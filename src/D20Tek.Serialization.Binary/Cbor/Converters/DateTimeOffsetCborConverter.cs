using System.Formats.Cbor;

namespace D20Tek.Serialization.Binary.Cbor.Converters;

/// <summary>
/// A CBOR converter for <see cref="DateTimeOffset"/> values. Uses the same encoding as
/// <see cref="DateTimeCborConverter"/>: CBOR <b>Tag 1</b> followed by a 64-bit integer
/// representing the number of milliseconds since the Unix epoch (1970-01-01T00:00:00Z).
/// The UTC component of the <see cref="DateTimeOffset"/> is used for encoding.
/// </summary>
public sealed class DateTimeOffsetCborConverter : Converter<DateTimeOffset>
{
    internal static readonly CborTag EpochTag = (CborTag)1;
    private static readonly DateTimeOffset UnixEpoch = new(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <inheritdoc />
    public override DateTimeOffset Read(IFormatReader reader, SerializerOptions options)
    {
        var cborReader = (CborFormatReader)reader;
        var tag = cborReader.ReadTag();
        if (tag != EpochTag)
        {
            throw new SerializationException(
                $"Expected CBOR tag {(ulong)EpochTag} for DateTimeOffset but found tag {(ulong)tag}.", "$");
        }

        var epochMs = reader.GetInt64();
        return UnixEpoch.AddMilliseconds(epochMs);
    }

    /// <inheritdoc />
    public override void Write(IFormatWriter writer, DateTimeOffset value, SerializerOptions options)
    {
        var cborWriter = (CborFormatWriter)writer;
        cborWriter.WriteTag(EpochTag);
        var epochMs = (long)(value.UtcDateTime - UnixEpoch).TotalMilliseconds;
        writer.WriteNumber(epochMs);
    }
}
