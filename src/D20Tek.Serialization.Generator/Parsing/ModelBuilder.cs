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

        foreach (var property in EnumerateProperties(type))
        {
            if (MemberAttributeResolver.IsIgnored(property))
            {
                continue;
            }

            if (property.SetMethod is null || property.SetMethod.DeclaredAccessibility != Accessibility.Public)
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
                property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                strategy,
                isNullable,
                MemberAttributeResolver.IsRequired(property),
                MemberAttributeResolver.ResolveAliases(property)));
        }

        return new SerializableTypeModel(
            type.Name,
            type.ContainingNamespace.IsGlobalNamespace ? null : type.ContainingNamespace.ToDisplayString(),
            type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            type.IsValueType,
            members.ToImmutable().ToEquatableArray(),
            diagnostics.ToImmutable().ToEquatableArray());
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

    private static DiagnosticInfo CreateDiagnostic(DiagnosticDescriptor descriptor, ISymbol symbol, params string[] args) =>
        new(
            descriptor,
            LocationInfo.CreateFrom(symbol.Locations.FirstOrDefault()),
            args.ToImmutableArray().ToEquatableArray());
}
