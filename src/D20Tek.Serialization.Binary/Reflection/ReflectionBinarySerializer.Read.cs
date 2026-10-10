using D20Tek.Serialization.Metadata;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Binary.Reflection;

internal sealed partial class ReflectionBinarySerializer
{
    [RequiresUnreferencedCode(ReflectionMessage)]
    [RequiresDynamicCode(ReflectionMessage)]
    private object? ReadObject(IFormatReader reader, Type declaredType, BinarySerializerOptions options, string parentPath)
    {
        var actualType = declaredType;

        reader.ReadStartObject();

        if (PolymorphicTypeResolver.TryResolve(declaredType, out var polymorphic))
        {
            if (!reader.TryReadPropertyName(out var firstName) ||
                firstName != polymorphic.DiscriminatorPropertyName)
            {
                throw new SerializationException(
                    $"Expected the type discriminator property '{polymorphic.DiscriminatorPropertyName}' " +
                    $"as the first member of a polymorphic '{declaredType}' value.",
                    parentPath);
            }

            var discriminator = reader.GetString();
            if (!polymorphic.TryGetType(discriminator, out var resolvedType))
            {
                throw new SerializationException(
                    $"Unknown type discriminator '{discriminator}' for polymorphic type " +
                    $"'{declaredType}'. Register the derived type with " +
                    $"[SerializedDerivedType(typeof(...), \"{discriminator}\")] on '{declaredType}'.",
                    parentPath);
            }

            actualType = resolvedType;
        }

        var metadata = _provider.GetOrAdd(actualType, options, options.IncludeFields);
        var instance = Activator.CreateInstance(actualType)!;
        var memberLookup = ConversionHelper.BuildMemberLookup(metadata);
        var seen = new HashSet<string>();

        while (reader.TryReadPropertyName(out var name))
        {
            if (memberLookup.TryGetValue(name, out var member))
            {
                var memberPath = $"{parentPath}.{member.SerializedName}";
                var value = ReadValue(reader, member.MemberType, options, memberPath);
                member.Setter(instance, value);
                seen.Add(member.Name);
            }
            else
            {
                reader.SkipValue();
            }
        }

        reader.ReadEndObject();

        // Enforce required members
        foreach (var member in metadata.Members)
        {
            if (member.IsRequired && !seen.Contains(member.Name))
            {
                var path = $"{parentPath}.{member.SerializedName}";
                throw new SerializationException(
                    $"Missing required property '{member.SerializedName}'.",
                    path);
            }
        }

        return instance;
    }

    [RequiresUnreferencedCode(ReflectionMessage)]
    [RequiresDynamicCode(ReflectionMessage)]
    private object? ReadValue(IFormatReader reader, Type declaredType, BinarySerializerOptions options, string path)
    {
        if (reader.IsNull()) return null;

        var actualType = ConversionHelper.GetUnderlyingType(declaredType);
        return actualType switch
        {
            _ when actualType == typeof(bool)       => reader.GetBoolean(),
            _ when actualType == typeof(string)     => reader.GetString(),
            _ when ConversionHelper.IsIntegerType(actualType)        =>
                ConversionHelper.ConvertInteger(reader.GetInt64(), actualType),
            _ when ConversionHelper.IsFloatingPointType(actualType)  =>
                ConversionHelper.ConvertFloat(reader.GetDouble(), actualType),
            _ when actualType.IsEnum                => Enum.ToObject(actualType, reader.GetInt64()),
            _                                       => ReadObject(reader, actualType, options, path)
        };
    }
}
