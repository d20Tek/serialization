using D20Tek.Serialization.Metadata;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Binary.Reflection;

/// <summary>
/// The reflection-based serialization fallback that uses <see cref="TypeMetadata"/> to
/// write and read objects through <see cref="IFormatWriter"/>/<see cref="IFormatReader"/>.
/// This is format-agnostic: it drives the abstract interfaces and does not reference CBOR
/// encoding details directly.
/// </summary>
/// <remarks>
/// This path is not AOT/trim-safe. It requires unreferenced code and dynamic code because it
/// uses reflection (via <see cref="TypeMetadataBuilder"/>) to discover and access members at
/// runtime.
/// </remarks>
internal sealed class ReflectionBinarySerializer
{
    private const string ReflectionMessage =
        "The reflection-based serialization fallback dynamically reflects over and accesses " +
        "type members and is not compatible with trimming or ahead-of-time compilation.";

    private readonly TypeMetadataProvider _provider = new();

    /// <summary>
    /// Serializes a value of type <typeparamref name="T"/> to the specified writer.
    /// </summary>
    [RequiresUnreferencedCode(ReflectionMessage)]
    [RequiresDynamicCode(ReflectionMessage)]
    public void Write<T>(IFormatWriter writer, T value, BinarySerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(options);

        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        WriteObject(writer, value, typeof(T), options);
    }

    /// <summary>
    /// Deserializes a value of type <typeparamref name="T"/> from the specified reader.
    /// </summary>
    [RequiresUnreferencedCode(ReflectionMessage)]
    [RequiresDynamicCode(ReflectionMessage)]
    public T? Read<T>(IFormatReader reader, BinarySerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(options);

        if (reader.IsNull()) return default;

        return (T?)ReadObject(reader, typeof(T), options, "$");
    }

    [RequiresUnreferencedCode(ReflectionMessage)]
    [RequiresDynamicCode(ReflectionMessage)]
    private void WriteObject(IFormatWriter writer, object instance, Type type, BinarySerializerOptions options)
    {
        var metadata = _provider.GetOrAdd(type, options, options.IncludeFields);

        writer.WriteStartObject();
        foreach (var member in metadata.Members)
        {
            var value = member.Getter(instance);

            if (value is null)
            {
                if (member.IgnoreNull) continue;

                writer.WritePropertyName(member.SerializedName);
                writer.WriteNull();
            }
            else
            {
                writer.WritePropertyName(member.SerializedName);
                WriteValue(writer, value, member.MemberType, options);
            }
        }

        writer.WriteEndObject();
    }

    [RequiresUnreferencedCode(ReflectionMessage)]
    [RequiresDynamicCode(ReflectionMessage)]
    private void WriteValue(IFormatWriter writer, object value, Type declaredType, BinarySerializerOptions options)
    {
        switch (value)
        {
            case bool b: writer.WriteBoolean(b); break;
            case string s: writer.WriteString(s); break;
            case byte or sbyte or short or ushort or int or uint or long or ulong:
                writer.WriteNumber(Convert.ToInt64(value)); break;
            case float or double or decimal: writer.WriteNumber(Convert.ToDouble(value)); break;
            case Enum: writer.WriteNumber(Convert.ToInt64(value)); break;
            default: WriteObject(writer, value, ConversionHelper.GetUnderlyingType(declaredType), options); break;
        }
    }

    [RequiresUnreferencedCode(ReflectionMessage)]
    [RequiresDynamicCode(ReflectionMessage)]
    private object? ReadObject(IFormatReader reader, Type type, BinarySerializerOptions options, string parentPath)
    {
        var metadata = _provider.GetOrAdd(type, options, options.IncludeFields);
        var instance = Activator.CreateInstance(type)!;
        var memberLookup = ConversionHelper.BuildMemberLookup(metadata);
        var seen = new HashSet<string>();

        reader.ReadStartObject();
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
