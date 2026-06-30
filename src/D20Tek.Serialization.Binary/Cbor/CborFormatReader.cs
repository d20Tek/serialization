using D20Tek.Serialization.Errors;
using System.Buffers.Binary;
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
internal sealed class CborFormatReader : IFormatReader
{
    private readonly CborReader _reader;
    private readonly BinaryDecodingMode _decodingMode;
    private readonly SerializationPathBuilder _path = new();
    private readonly Stack<Scope> _scopes = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CborFormatReader"/> class over the supplied
    /// CBOR document.
    /// </summary>
    /// <param name="data">
    /// The CBOR-encoded source buffer. The buffer must remain alive and unmodified for as long
    /// as any span returned by <see cref="GetRawStringBytes"/> or <see cref="GetRawNumberBytes"/>
    /// is in use, because those spans reference this buffer directly.
    /// </param>
    /// <param name="decodingMode">
    /// The decoding mode that controls how unknown tags are handled. The default is
    /// <see cref="BinaryDecodingMode.Lenient"/>.
    /// </param>
    public CborFormatReader(ReadOnlyMemory<byte> data, BinaryDecodingMode decodingMode = BinaryDecodingMode.Lenient)
    {
        _decodingMode = decodingMode;
        _reader = new CborReader(data, CborConformanceMode.Lax, allowMultipleRootLevelValues: false);
    }

    /// <summary>
    /// Gets the decoding mode that governs how unknown tags are handled.
    /// </summary>
    public BinaryDecodingMode DecodingMode => _decodingMode;

    /// <inheritdoc />
    public ValueKind ValueKind => MapState(_reader.PeekState());

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
        double value;
        switch (_reader.PeekState())
        {
            case CborReaderState.UnsignedInteger:
            case CborReaderState.NegativeInteger:
                value = _reader.ReadInt64();
                break;
            case CborReaderState.HalfPrecisionFloat:
                value = (double)_reader.ReadHalf();
                break;
            case CborReaderState.SinglePrecisionFloat:
                value = _reader.ReadSingle();
                break;
            case CborReaderState.DoublePrecisionFloat:
                value = _reader.ReadDouble();
                break;
            default:
                throw Mismatch(ValueKind.Number);
        }

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
        var (offset, length) = GetStringPayloadRange(encoded);
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

    private void EnsureStringKey()
    {
        var state = _reader.PeekState();
        if (state == CborReaderState.StartIndefiniteLengthTextString) throw IndefiniteNotSupported("text string");
        if (state != CborReaderState.TextString) throw Mismatch(ValueKind.String);
    }

    private void PushPropertySegment(string name)
    {
        var scope = _scopes.Count > 0 ? _scopes.Peek() : null;
        if (scope is { HasChild: true }) _path.Pop();

        _path.PushProperty(name);
        if (scope is not null) scope.HasChild = true;
    }

    private void OnValueRead()
    {
        if (_scopes.Count == 0) return;

        var scope = _scopes.Peek();
        if (scope.IsArray)
        {
            scope.Index++;
            _path.SetIndex(scope.Index);
        }
    }

    private SerializationException Mismatch(ValueKind expected)
    {
        var actual = MapState(_reader.PeekState());
        return new SerializationException(
            $"Expected {expected} but found {actual}.",
            _path.ToPath(),
            expected,
            actual);
    }

    private SerializationException IndefiniteNotSupported(string itemKind) =>
        new($"Indefinite-length {itemKind} values are not supported.", _path.ToPath());

    private static ValueKind MapState(CborReaderState state) => state switch
    {
        CborReaderState.StartMap => ValueKind.Object,
        CborReaderState.StartArray => ValueKind.Array,
        CborReaderState.TextString or CborReaderState.StartIndefiniteLengthTextString => ValueKind.String,
        CborReaderState.ByteString or CborReaderState.StartIndefiniteLengthByteString => ValueKind.String,
        CborReaderState.UnsignedInteger or CborReaderState.NegativeInteger => ValueKind.Number,
        CborReaderState.HalfPrecisionFloat or CborReaderState.SinglePrecisionFloat
            or CborReaderState.DoublePrecisionFloat => ValueKind.Number,
        CborReaderState.Boolean => ValueKind.Boolean,
        _ => ValueKind.Null,
    };

    /// <summary>
    /// Decodes the length header of a CBOR text or byte string and returns the payload's byte
    /// offset and length within <paramref name="encoded"/>. Exposed as <see langword="internal"/>
    /// so every header form — including the defensive malformed case that the public read path
    /// rejects earlier — can be unit tested directly.
    /// </summary>
    /// <param name="encoded">The full encoded string item, beginning with its initial byte.</param>
    /// <returns>The payload's offset and length within <paramref name="encoded"/>.</returns>
    /// <exception cref="SerializationException">Thrown when the length header is malformed.</exception>
    internal static (int Offset, int Length) GetStringPayloadRange(ReadOnlySpan<byte> encoded)
    {
        int additionalInfo = encoded[0] & 0x1F;
        return additionalInfo switch
        {
            <= 23 => (1, additionalInfo),
            24 => (2, encoded[1]),
            25 => (3, BinaryPrimitives.ReadUInt16BigEndian(encoded.Slice(1, 2))),
            26 => (5, checked((int)BinaryPrimitives.ReadUInt32BigEndian(encoded.Slice(1, 4)))),
            27 => (9, checked((int)BinaryPrimitives.ReadUInt64BigEndian(encoded.Slice(1, 8)))),
            _ => throw new SerializationException("Malformed CBOR string length header.", "$"),
        };
    }

    private sealed class Scope(bool isArray)
    {
        public bool IsArray { get; } = isArray;

        public int Index { get; set; }

        public bool HasChild { get; set; }
    }
}
