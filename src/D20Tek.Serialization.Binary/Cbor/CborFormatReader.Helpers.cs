using System.Formats.Cbor;

namespace D20Tek.Serialization.Binary.Cbor;

internal partial class CborFormatReader
{
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
        scope?.HasChild = true;
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

    private SerializationException Mismatch(ValueKind expected) => CborReadErrors.Mismatch(expected, _reader.PeekState(), _path.ToPath());

    private SerializationException IndefiniteNotSupported(string itemKind) => CborReadErrors.IndefiniteNotSupported(itemKind, _path.ToPath());

    private sealed class Scope(bool isArray)
    {
        public bool IsArray { get; } = isArray;

        public int Index { get; set; }

        public bool HasChild { get; set; }
    }
}
