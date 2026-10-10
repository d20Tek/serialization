using D20Tek.Serialization.Metadata;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Binary.Reflection;

internal sealed partial class ReflectionBinarySerializer
{
    [RequiresUnreferencedCode(ReflectionMessage)]
    [RequiresDynamicCode(ReflectionMessage)]
    private void WriteObject(IFormatWriter writer, object instance, Type declaredType, BinarySerializerOptions options)
    {
        var actualType = declaredType;
        if (PolymorphicTypeResolver.TryResolve(declaredType, out var polymorphic))
        {
            actualType = instance.GetType();
        }

        var metadata = _provider.GetOrAdd(actualType, options, options.IncludeFields);
        writer.WriteStartObject();

        if (polymorphic is not null)
        {
            if (!polymorphic.TryGetDiscriminator(actualType, out var discriminator))
            {
                throw new SerializationException(
                    $"Type '{actualType}' is not registered as a derived type of polymorphic " +
                    $"type '{declaredType}'. Annotate '{declaredType}' with " +
                    $"[SerializedDerivedType(typeof({actualType.Name}), \"...\")].",
                    "$");
            }

            writer.WritePropertyName(polymorphic.DiscriminatorPropertyName);
            writer.WriteString(discriminator);
        }

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
}
