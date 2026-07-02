using System.Formats.Cbor;

namespace D20Tek.Serialization.Binary.Cbor.Converters;

/// <summary>
/// A CBOR converter for <see cref="Guid"/> values. Encodes as CBOR <b>Tag 37</b> followed by a
/// 16-byte byte string (the GUID's binary representation in big-endian RFC 9562 format).
/// </summary>
public sealed class GuidCborConverter : Converter<Guid>
{
    /// <summary>
    /// The CBOR semantic tag for UUID (RFC 9562 / formerly RFC 4122).
    /// </summary>
    internal static readonly CborTag UuidTag = (CborTag)37;

    /// <inheritdoc />
    public override Guid Read(IFormatReader reader, SerializerOptions options)
    {
        var cborReader = (CborFormatReader)reader;
        var tag = cborReader.ReadTag();
        if (tag != UuidTag)
        {
            throw new SerializationException(
                $"Expected CBOR tag {(ulong)UuidTag} for Guid but found tag {(ulong)tag}.", "$");
        }

        var bytes = cborReader.ReadByteString();
        return new Guid(bytes, bigEndian: true);
    }

    /// <inheritdoc />
    public override void Write(IFormatWriter writer, Guid value, SerializerOptions options)
    {
        var cborWriter = (CborFormatWriter)writer;
        cborWriter.WriteTag(UuidTag);
        Span<byte> bytes = stackalloc byte[16];
        value.TryWriteBytes(bytes, bigEndian: true, out _);
        cborWriter.WriteByteString(bytes);
    }
}
