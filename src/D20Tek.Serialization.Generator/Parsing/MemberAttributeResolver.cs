using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace D20Tek.Serialization.Generation;

/// <summary>
/// Resolves the serialization-related attributes (<c>[NameSerialized]</c>, <c>[IgnoreSerialized]</c>,
/// <c>[RequiredSerialized]</c>) applied to a property.
/// </summary>
internal static class MemberAttributeResolver
{
    private const string SerializedNameAttribute = "D20Tek.Serialization.NameSerializedAttribute";
    private const string IgnoreSerializedAttribute = "D20Tek.Serialization.IgnoreSerializedAttribute";
    private const string RequiredSerializedAttribute = "D20Tek.Serialization.RequiredSerializedAttribute";

    public static string? ResolveSerializedName(IPropertySymbol property)
    {
        foreach (var attribute in property.GetAttributes())
        {
            if (attribute.AttributeClass!.ToDisplayString() == SerializedNameAttribute &&
                attribute.ConstructorArguments.Length >= 1 &&
                attribute.ConstructorArguments[0].Value is string name)
            {
                return name;
            }
        }

        return null;
    }

    public static EquatableArray<string> ResolveAliases(IPropertySymbol property)
    {
        foreach (var attribute in property.GetAttributes())
        {
            if (attribute.AttributeClass!.ToDisplayString() != SerializedNameAttribute)
            {
                continue;
            }

            // The aliases are passed via the `params string[] aliases` constructor parameter,
            // which Roslyn surfaces as the second constructor argument whose kind is Array.
            if (attribute.ConstructorArguments.Length >= 2 &&
                attribute.ConstructorArguments[1].Kind == TypedConstantKind.Array &&
                !attribute.ConstructorArguments[1].IsNull)
            {
                var values = attribute.ConstructorArguments[1].Values
                    .Select(v => v.Value as string)
                    .Where(v => v is not null)
                    .Select(v => v!)
                    .ToImmutableArray();

                return values.ToEquatableArray();
            }
        }

        return EquatableArray<string>.Empty;
    }

    public static bool IsIgnored(IPropertySymbol property) => HasAttribute(property, IgnoreSerializedAttribute);

    public static bool IsRequired(IPropertySymbol property) => HasAttribute(property, RequiredSerializedAttribute);

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
}
