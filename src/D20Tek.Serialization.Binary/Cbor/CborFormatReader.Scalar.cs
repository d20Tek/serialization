using System.Formats.Cbor;

namespace D20Tek.Serialization.Binary.Cbor;

internal sealed partial class CborFormatReader
{
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
