using System.Formats.Cbor;

namespace D20Tek.Serialization.Binary.Cbor.Converters;

/// <summary>
/// A CBOR converter for <see cref="DateTime"/> values. Encodes as CBOR <b>Tag 1</b> (epoch-based
/// date/time) followed by a 64-bit integer representing the number of milliseconds since the
/// Unix epoch (1970-01-01T00:00:00Z). Values are normalized to UTC before encoding.
/// </summary>
public sealed class DateTimeCborConverter : Converter<DateTime>
{
    internal static readonly CborTag EpochTag = (CborTag)1;
    private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override DateTime Read(IFormatReader reader, SerializerOptions options)
    {
        var cborReader = (CborFormatReader)reader;
        var tag = cborReader.ReadTag();
        if (tag != EpochTag)
        {
            throw new SerializationException(
                $"Expected CBOR tag {(ulong)EpochTag} for DateTime but found tag {(ulong)tag}.", "$");
        }

        var epochMs = reader.GetInt64();
        return UnixEpoch.AddMilliseconds(epochMs);
    }

    /// <inheritdoc />
    public override void Write(IFormatWriter writer, DateTime value, SerializerOptions options)
    {
        var cborWriter = (CborFormatWriter)writer;
        cborWriter.WriteTag(EpochTag);
        var utc = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
        var epochMs = (long)(utc - UnixEpoch).TotalMilliseconds;
        writer.WriteNumber(epochMs);
    }
}
