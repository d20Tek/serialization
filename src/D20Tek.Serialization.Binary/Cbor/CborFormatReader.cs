using D20Tek.Serialization.Errors;
using System.Formats.Cbor;

namespace D20Tek.Serialization.Binary.Cbor;

/// <summary>
/// A CBOR implementation of <see cref="IFormatReader"/> built on top of
/// <see cref="CborReader"/>. Only definite-length encodings are accepted; indefinite-length
/// items are rejected as a v1 invariant (spec §3.1).
/// </summary>
/// <remarks>
/// The reader provides zero-copy access to string and number payloads via
/// <see cref="GetRawStringBytes"/> and <see cref="GetRawNumberBytes"/>, which return slices of
/// the original source buffer without allocating managed strings (spec §3.4). A
/// <see cref="SerializationPathBuilder"/> tracks the current object property names and array
/// indices so that a JSONPath-style location (for example <c>$.items[3].price</c>) can be
/// attached to any <see cref="SerializationException"/> raised on a type mismatch (spec §3.5).
/// The configured <see cref="DecodingMode"/> controls how unknown semantic tags are treated
/// when values are skipped: <see cref="BinaryDecodingMode.Strict"/> rejects them while
/// <see cref="BinaryDecodingMode.Lenient"/> skips them.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="CborFormatReader"/> class over the supplied
/// CBOR document.
/// </remarks>
/// <param name="data">
/// The CBOR-encoded source buffer. The buffer must remain alive and unmodified for as long
/// as any span returned by <see cref="GetRawStringBytes"/> or <see cref="GetRawNumberBytes"/>
/// is in use, because those spans reference this buffer directly.
/// </param>
/// <param name="decodingMode">
/// The decoding mode that controls how unknown tags are handled. The default is
/// <see cref="BinaryDecodingMode.Lenient"/>.
/// </param>
internal sealed partial class CborFormatReader(ReadOnlyMemory<byte> data, BinaryDecodingMode decodingMode = BinaryDecodingMode.Lenient)
    : IFormatReader
{
    private readonly CborReader _reader = new(data, CborConformanceMode.Lax, allowMultipleRootLevelValues: false);
    private readonly BinaryDecodingMode _decodingMode = decodingMode;
    private readonly SerializationPathBuilder _path = new();
    private readonly Stack<Scope> _scopes = new();

    /// <summary>
    /// Gets the decoding mode that governs how unknown tags are handled.
    /// </summary>
    public BinaryDecodingMode DecodingMode => _decodingMode;

    /// <inheritdoc />
    public ValueKind ValueKind => CborReadErrors.MapState(_reader.PeekState());

    /// <inheritdoc />
    public void ReadStartObject()
    {
        if (_reader.PeekState() != CborReaderState.StartMap) throw Mismatch(ValueKind.Object);

        // CborReaderState reports StartMap for both definite- and indefinite-length maps;
        // ReadStartMap returns null for the indefinite-length form, which v1 rejects.
        if (_reader.ReadStartMap() is null) throw IndefiniteNotSupported("map");

        _scopes.Push(new Scope(isArray: false));
    }

    /// <inheritdoc />
    public void ReadEndObject()
    {
        _reader.ReadEndMap();
        var scope = _scopes.Pop();
        if (scope.HasChild) _path.Pop();
        OnValueRead();
    }

    /// <inheritdoc />
    public void ReadStartArray()
    {
        if (_reader.PeekState() != CborReaderState.StartArray) throw Mismatch(ValueKind.Array);

        // CborReaderState reports StartArray for both definite- and indefinite-length arrays;
        // ReadStartArray returns null for the indefinite-length form, which v1 rejects.
        if (_reader.ReadStartArray() is null) throw IndefiniteNotSupported("array");

        _scopes.Push(new Scope(isArray: true));
        _path.PushIndex(0);
    }

    /// <inheritdoc />
    public void ReadEndArray()
    {
        _reader.ReadEndArray();
        _scopes.Pop();
        _path.Pop();
        OnValueRead();
    }

    /// <inheritdoc />
    public bool TryReadPropertyName(out string name)
    {
        if (_reader.PeekState() == CborReaderState.EndMap)
        {
            name = string.Empty;
            return false;
        }

        EnsureStringKey();
        name = _reader.ReadTextString();
        PushPropertySegment(name);
        return true;
    }

    /// <inheritdoc />
    public bool IsNull()
    {
        if (_reader.PeekState() != CborReaderState.Null) return false;

        _reader.ReadNull();
        OnValueRead();
        return true;
    }

    /// <inheritdoc />
    public bool GetBoolean()
    {
        if (_reader.PeekState() != CborReaderState.Boolean) throw Mismatch(ValueKind.Boolean);

        var value = _reader.ReadBoolean();
        OnValueRead();
        return value;
    }

    /// <inheritdoc />
    public long GetInt64()
    {
        var state = _reader.PeekState();
        if (state is not (CborReaderState.UnsignedInteger or CborReaderState.NegativeInteger))
        {
            throw Mismatch(ValueKind.Number);
        }

        var value = _reader.ReadInt64();
        OnValueRead();
        return value;
    }

    /// <inheritdoc />
    public double GetDouble()
    {
        var value = _reader.PeekState() switch
        {
            CborReaderState.UnsignedInteger or CborReaderState.NegativeInteger => _reader.ReadInt64(),
            CborReaderState.HalfPrecisionFloat => (double)_reader.ReadHalf(),
            CborReaderState.SinglePrecisionFloat => (double)_reader.ReadSingle(),
            CborReaderState.DoublePrecisionFloat => _reader.ReadDouble(),
            _ => throw Mismatch(ValueKind.Number),
        };
        OnValueRead();
        return value;
    }

    /// <inheritdoc />
    public string GetString()
    {
        var state = _reader.PeekState();
        if (state == CborReaderState.StartIndefiniteLengthTextString) throw IndefiniteNotSupported("text string");
        if (state != CborReaderState.TextString) throw Mismatch(ValueKind.String);

        var value = _reader.ReadTextString();
        OnValueRead();
        return value;
    }

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

    /// <summary>
    /// Skips the current value, including any nested object or array. When the value carries a
    /// semantic tag, the configured <see cref="DecodingMode"/> determines the behavior:
    /// <see cref="BinaryDecodingMode.Strict"/> rejects the unknown tag by throwing a
    /// <see cref="SerializationException"/>, while <see cref="BinaryDecodingMode.Lenient"/> skips
    /// the tagged value. This is used to discard the values of unknown object properties.
    /// </summary>
    /// <exception cref="SerializationException">
    /// Thrown when an unknown tag is encountered while decoding in
    /// <see cref="BinaryDecodingMode.Strict"/> mode.
    /// </exception>
    public void SkipValue()
    {
        if (_reader.PeekState() == CborReaderState.Tag && _decodingMode == BinaryDecodingMode.Strict)
        {
            var tag = _reader.PeekTag();
            throw new SerializationException(
                $"Unknown tag '{(ulong)tag}' is not supported in strict decoding mode.",
                _path.ToPath());
        }

        _reader.SkipValue();
        OnValueRead();
    }

    /// <summary>
    /// Reads and returns the next CBOR semantic tag (major type 6). The tag value is consumed
    /// and the reader advances so that the next read returns the tag's data item. Used by
    /// built-in converters to verify expected tags before reading tagged content.
    /// </summary>
    /// <returns>The <see cref="CborTag"/> that was read.</returns>
    public CborTag ReadTag() => _reader.ReadTag();

    /// <summary>
    /// Reads the current byte string (CBOR major type 2) and advances the reader. Used by
    /// converters that encode binary payloads (for example <c>GuidCborConverter</c>).
    /// </summary>
    /// <returns>The byte array read from the CBOR stream.</returns>
    public byte[] ReadByteString()
    {
        var value = _reader.ReadByteString();
        OnValueRead();
        return value;
    }
}
