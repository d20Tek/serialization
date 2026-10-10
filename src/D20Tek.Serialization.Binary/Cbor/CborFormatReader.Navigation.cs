using System.Formats.Cbor;

namespace D20Tek.Serialization.Binary.Cbor;

internal sealed partial class CborFormatReader
{
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
}
