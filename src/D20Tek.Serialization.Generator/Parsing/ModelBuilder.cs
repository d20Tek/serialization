using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace D20Tek.Serialization.Generation;

/// <summary>
/// Builds a value-equatable <see cref="SerializableTypeModel"/> from an <see cref="INamedTypeSymbol"/>
/// annotated with <c>[Serializable]</c>, applying the serialization attributes and classifying each
/// member's emit strategy. Unsupported members are reported as diagnostics and excluded.
/// </summary>
internal static class ModelBuilder
{
    public static SerializableTypeModel Build(INamedTypeSymbol type)
    {
        var diagnostics = ImmutableArray.CreateBuilder<DiagnosticInfo>();
        var members = ImmutableArray.CreateBuilder<MemberModel>();

        if (type.ContainingType is not null)
        {
            diagnostics.Add(new DiagnosticInfo(
                DiagnosticDescriptors.UnsupportedSerializableType,
                LocationInfo.CreateFrom(type.Locations.FirstOrDefault()),
                new[] { type.Name }.ToImmutableArray().ToEquatableArray()));
        }

        var constructorParameters = ResolveConstructorParameters(type);

        foreach (var property in EnumerateProperties(type))
        {
            if (MemberAttributeResolver.IsIgnored(property))
            {
                continue;
            }

            var constructorParameterName = constructorParameters is not null &&
                constructorParameters.TryGetValue(property.Name, out var parameterName)
                ? parameterName
                : null;

            if ((property.SetMethod is null || property.SetMethod.DeclaredAccessibility != Accessibility.Public) &&
                constructorParameterName is null)
            {
                diagnostics.Add(CreateDiagnostic(
                    DiagnosticDescriptors.InaccessibleSetter, property, type.Name, property.Name));
                continue;
            }

            if (!MemberStrategyClassifier.TryClassify(property.Type, out var strategy, out var isNullable))
            {
                diagnostics.Add(CreateDiagnostic(
                    DiagnosticDescriptors.UnsupportedMemberType,
                    property,
                    type.Name,
                    property.Name,
                    property.Type.ToDisplayString()));
                continue;
            }

            members.Add(new MemberModel(
                property.Name,
                MemberAttributeResolver.ResolveSerializedName(property),
                FullyQualifiedTypeName(property.Type),
                strategy,
                isNullable,
                MemberAttributeResolver.IsRequired(property),
                MemberAttributeResolver.ResolveAliases(property),
                property.SetMethod?.IsInitOnly ?? false,
                constructorParameterName));
        }

        return new SerializableTypeModel(
            type.Name,
            type.ContainingNamespace.IsGlobalNamespace ? null : type.ContainingNamespace.ToDisplayString(),
            type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            type.IsValueType,
            members.ToImmutable().ToEquatableArray(),
            diagnostics.ToImmutable().ToEquatableArray());
    }

    /// <summary>
    /// When <paramref name="type"/> has no accessible parameterless constructor, finds a public
    /// constructor whose parameters can be matched (by name, case-insensitively) to the type's
    /// serializable properties, so deserialization can construct the instance directly. Returns
    /// <see langword="null"/> when a parameterless constructor is available (the existing
    /// object-initializer/post-construction-assignment path should be used instead) or when no
    /// suitable constructor can be found.
    /// </summary>
    private static Dictionary<string, string>? ResolveConstructorParameters(INamedTypeSymbol type)
    {
        var constructors = type.Constructors
            .Where(c => !c.IsStatic && c.DeclaredAccessibility == Accessibility.Public)
            .ToList();

        if (constructors.Any(c => c.Parameters.Length == 0)) return null;

        var properties = type.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => !p.IsStatic && !p.IsIndexer && p.DeclaredAccessibility == Accessibility.Public)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var constructor in constructors.OrderByDescending(c => c.Parameters.Length))
        {
            var mapping = new Dictionary<string, string>(StringComparer.Ordinal);
            var matched = true;

            foreach (var parameter in constructor.Parameters)
            {
                if (!properties.TryGetValue(parameter.Name, out var property) ||
                    !SymbolEqualityComparer.Default.Equals(property.Type, parameter.Type))
                {
                    matched = false;
                    break;
                }

                mapping[property.Name] = parameter.Name;
            }

            if (matched) return mapping;
        }

        return null;
    }


    private static IEnumerable<IPropertySymbol> EnumerateProperties(INamedTypeSymbol type)
    {
        foreach (var member in type.GetMembers())
        {
            if (member is IPropertySymbol
                {
                    IsStatic: false,
                    IsIndexer: false,
                    DeclaredAccessibility: Accessibility.Public,
                    GetMethod: not null,
                } property && property.GetMethod!.DeclaredAccessibility == Accessibility.Public)
            {
                yield return property;
            }
        }
    }

    private static string FullyQualifiedTypeName(ITypeSymbol type)
    {
        var format = SymbolDisplayFormat.FullyQualifiedFormat
            .WithMiscellaneousOptions(
                SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions |
                SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

        return type.ToDisplayString(format);
    }

    private static DiagnosticInfo CreateDiagnostic(DiagnosticDescriptor descriptor, ISymbol symbol, params string[] args) =>
        new(
            descriptor,
            LocationInfo.CreateFrom(symbol.Locations.FirstOrDefault()),
            args.ToImmutableArray().ToEquatableArray());
}