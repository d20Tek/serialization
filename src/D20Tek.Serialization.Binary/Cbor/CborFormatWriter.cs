using System.Buffers;
using System.Buffers.Binary;
using System.Formats.Cbor;

namespace D20Tek.Serialization.Binary.Cbor;

/// <summary>
/// A CBOR implementation of <see cref="IFormatWriter"/> built on top of
/// <see cref="CborWriter"/>. Containers are emitted using definite-length encodings only;
/// indefinite-length encodings are disallowed in v1.
/// </summary>
/// <remarks>
/// The <see cref="IFormatWriter"/> contract starts objects and arrays without an element
/// count, so the underlying writer is configured to accept indefinite-length container calls
/// and convert them into definite-length encodings when the document is encoded. Lax
/// conformance is used so that map keys keep their insertion order instead of being sorted.
/// Floating-point values are always emitted as full 64-bit IEEE-754 by writing a
/// pre-encoded value, because the underlying writer's <c>WriteDouble</c> otherwise applies
/// preferred (shortest) float encoding that would reduce values such as 1.0 to half precision.
/// Integers are still emitted in their shortest form.
/// </remarks>
internal sealed class CborFormatWriter : IFormatWriter
{
    private readonly CborWriter _writer = new(CborConformanceMode.Lax, convertIndefiniteLengthEncodings: true);

    /// <inheritdoc />
    public void WriteStartObject() => _writer.WriteStartMap(null);

    /// <inheritdoc />
    public void WriteEndObject() => _writer.WriteEndMap();

    /// <inheritdoc />
    public void WriteStartArray() => _writer.WriteStartArray(null);

    /// <inheritdoc />
    public void WriteEndArray() => _writer.WriteEndArray();

    /// <inheritdoc />
    public void WritePropertyName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        _writer.WriteTextString(name);
    }

    /// <inheritdoc />
    public void WriteNull() => _writer.WriteSimpleValue(CborSimpleValue.Null);

    /// <inheritdoc />
    public void WriteBoolean(bool value) => _writer.WriteBoolean(value);

    /// <inheritdoc />
    public void WriteNumber(long value) => _writer.WriteInt64(value);

    /// <inheritdoc />
    /// <remarks>
    /// Always emits a 64-bit (double-precision) IEEE-754 value (CBOR major type 7, value 27)
    /// per spec §3.1, bypassing the writer's preferred-float reduction.
    /// </remarks>
    public void WriteNumber(double value)
    {
        Span<byte> encoded = stackalloc byte[sizeof(double) + 1];
        encoded[0] = 0xFB;
        BinaryPrimitives.WriteUInt64BigEndian(encoded[1..], BitConverter.DoubleToUInt64Bits(value));
        _writer.WriteEncodedValue(encoded);
    }

    /// <inheritdoc />
    public void WriteString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _writer.WriteTextString(value);
    }

    /// <summary>
    /// Writes a raw byte string (CBOR major type 2). This is the encoding used for
    /// <see cref="byte"/>-array values and by tag-based converters that emit binary payloads.
    /// </summary>
    /// <param name="value">The bytes to write as a CBOR byte string.</param>
    public void WriteByteString(ReadOnlySpan<byte> value) => _writer.WriteByteString(value);

    /// <summary>
    /// Writes a CBOR semantic tag (major type 6). The tag must be immediately followed by
    /// the data item it annotates. Used by built-in converters such as
    /// <c>GuidCborConverter</c> (Tag 37) and <c>DateTimeCborConverter</c> (Tag 1).
    /// </summary>
    /// <param name="tag">The CBOR tag to write.</param>
    public void WriteTag(CborTag tag) => _writer.WriteTag(tag);

    /// <summary>
    /// Encodes the written document into a new byte array using definite-length encodings.
    /// </summary>
    /// <returns>The CBOR-encoded document.</returns>
    public byte[] Encode() => _writer.Encode();

    /// <summary>
    /// Encodes the written document into the supplied <paramref name="bufferWriter"/> using
    /// definite-length encodings.
    /// </summary>
    /// <param name="bufferWriter">The buffer writer that receives the encoded document.</param>
    public void Encode(IBufferWriter<byte> bufferWriter)
    {
        ArgumentNullException.ThrowIfNull(bufferWriter);
        bufferWriter.Write(_writer.Encode());
    }
}
