using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace D20Tek.Serialization.Generation;

/// <summary>
/// Builds a value-equatable <see cref="SerializableTypeModel"/> from an <see cref="INamedTypeSymbol"/>
/// annotated with <c>[Serializable]</c>, applying the serialization attributes and classifying each
/// member's emit strategy. Unsupported members are reported as diagnostics and excluded.
/// </summary>
internal static class ModelBuilder
{
    private const string SerializedNameAttribute = "D20Tek.Serialization.SerializedNameAttribute";
    private const string IgnoreSerializedAttribute = "D20Tek.Serialization.IgnoreSerializedAttribute";
    private const string RequiredSerializedAttribute = "D20Tek.Serialization.RequiredSerializedAttribute";

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
            if (IsIgnored(property))
            {
                continue;
            }

            if (property.SetMethod is null || property.SetMethod.DeclaredAccessibility != Accessibility.Public)
            {
                diagnostics.Add(CreateDiagnostic(
                    DiagnosticDescriptors.InaccessibleSetter, property, type.Name, property.Name));
                continue;
            }

            if (!TryClassify(property.Type, out var strategy, out var isNullable))
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
                ResolveSerializedName(property),
                property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                strategy,
                isNullable,
                IsRequired(property)));
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

    private static bool TryClassify(ITypeSymbol type, out MemberStrategy strategy, out bool isNullable)
    {
        isNullable = false;

        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable &&
            nullable.TypeArguments.Length == 1)
        {
            isNullable = true;
            return TryClassifyCore(nullable.TypeArguments[0], out strategy);
        }

        if (type.IsReferenceType)
        {
            isNullable = type.NullableAnnotation != NullableAnnotation.NotAnnotated;
        }

        return TryClassifyCore(type, out strategy);
    }

    private static bool TryClassifyCore(ITypeSymbol type, out MemberStrategy strategy)
    {
        if (type.TypeKind == TypeKind.Enum)
        {
            strategy = MemberStrategy.Enum;
            return true;
        }

        switch (type.SpecialType)
        {
            case SpecialType.System_Boolean:
                strategy = MemberStrategy.Boolean;
                return true;
            case SpecialType.System_Byte:
            case SpecialType.System_SByte:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            case SpecialType.System_Int64:
            case SpecialType.System_UInt64:
                strategy = MemberStrategy.Int64;
                return true;
            case SpecialType.System_Single:
            case SpecialType.System_Double:
                strategy = MemberStrategy.Double;
                return true;
            case SpecialType.System_String:
                strategy = MemberStrategy.String;
                return true;
            default:
                strategy = default;
                return false;
        }
    }

    private static string? ResolveSerializedName(IPropertySymbol property)
    {
        foreach (var attribute in property.GetAttributes())
        {
            if (attribute.AttributeClass!.ToDisplayString() == SerializedNameAttribute &&
                attribute.ConstructorArguments.Length == 1 &&
                attribute.ConstructorArguments[0].Value is string name)
            {
                return name;
            }
        }

        return null;
    }

    private static bool IsIgnored(IPropertySymbol property) => HasAttribute(property, IgnoreSerializedAttribute);

    private static bool IsRequired(IPropertySymbol property) => HasAttribute(property, RequiredSerializedAttribute);

    private static bool HasAttribute(ISymbol symbol, string attributeMetadataName)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass!.ToDisplayString() == attributeMetadataName)
            {
                return true;
            }
        }

        return false;
    }

    private static DiagnosticInfo CreateDiagnostic(DiagnosticDescriptor descriptor, ISymbol symbol, params string[] args) =>
        new(
            descriptor,
            LocationInfo.CreateFrom(symbol.Locations.FirstOrDefault()),
            args.ToImmutableArray().ToEquatableArray());
}
