namespace D20Tek.Serialization.Binary.Cbor.Converters;

/// <summary>
/// A CBOR converter for enum values of type <typeparamref name="TEnum"/>. Encodes the enum
/// as its underlying 64-bit integer representation. This converter is generic and must be
/// explicitly registered by the user for each enum type; it is not auto-registered.
/// </summary>
/// <typeparam name="TEnum">The enum type to convert.</typeparam>
public sealed class EnumCborConverter<TEnum> : Converter<TEnum>
    where TEnum : struct, Enum
{
    /// <inheritdoc />
    public override TEnum Read(IFormatReader reader, SerializerOptions options)
    {
        var raw = reader.GetInt64();
        return (TEnum)Enum.ToObject(typeof(TEnum), raw);
    }

    /// <inheritdoc />
    public override void Write(IFormatWriter writer, TEnum value, SerializerOptions options)
    {
        writer.WriteNumber(Convert.ToInt64(value));
    }
}
