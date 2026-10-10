using System.Formats.Cbor;

namespace D20Tek.Serialization.Binary.Cbor;

internal sealed partial class CborFormatReader
{
    /// <inheritdoc />
    /// <remarks>
    /// Returns a slice of the source buffer covering the raw UTF-8 payload (for a text string)
    /// or the raw bytes (for a byte string), excluding the CBOR header. No managed string is
    /// allocated. Indefinite-length strings are rejected.
    /// </remarks>
    public ReadOnlySpan<byte> GetRawStringBytes()
    {
        var state = _reader.PeekState();
        if (state is CborReaderState.StartIndefiniteLengthTextString or CborReaderState.StartIndefiniteLengthByteString)
        {
            throw IndefiniteNotSupported("string");
        }

        if (state is not (CborReaderState.TextString or CborReaderState.ByteString))
        {
            throw Mismatch(ValueKind.String);
        }

        var encoded = _reader.ReadEncodedValue().Span;
        var (offset, length) = CborLengthDecoder.GetStringPayloadRange(encoded);
        var payload = encoded.Slice(offset, length);
        OnValueRead();
        return payload;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Returns a slice of the source buffer covering the complete CBOR encoding of the current
    /// number (header and payload), without allocating. For small integers the value is encoded
    /// entirely within the initial byte, so the full item is the natural numeric slice.
    /// </remarks>
    public ReadOnlySpan<byte> GetRawNumberBytes()
    {
        var state = _reader.PeekState();
        if (state is not (CborReaderState.UnsignedInteger or CborReaderState.NegativeInteger
            or CborReaderState.HalfPrecisionFloat or CborReaderState.SinglePrecisionFloat
            or CborReaderState.DoublePrecisionFloat))
        {
            throw Mismatch(ValueKind.Number);
        }

        var encoded = _reader.ReadEncodedValue().Span;
        OnValueRead();
        return encoded;
    }
}
