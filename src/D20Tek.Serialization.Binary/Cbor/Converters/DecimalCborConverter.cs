using System.Globalization;

namespace D20Tek.Serialization.Binary.Cbor.Converters;

/// <summary>
/// A CBOR converter for <see cref="decimal"/> values. Encodes as a CBOR text string because
/// CBOR has no native decimal type, and string encoding preserves full decimal precision
/// without the rounding artifacts of IEEE-754 double conversion.
/// </summary>
public sealed class DecimalCborConverter : Converter<decimal>
{
    /// <inheritdoc />
    public override decimal Read(IFormatReader reader, SerializerOptions options)
    {
        var text = reader.GetString();
        return decimal.Parse(text, CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public override void Write(IFormatWriter writer, decimal value, SerializerOptions options)
    {
        writer.WriteString(value.ToString(CultureInfo.InvariantCulture));
    }
}
