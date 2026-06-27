using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Core.Tests.Fakes;

[ExcludeFromCodeCoverage]
internal sealed class ScriptedFormatReader(string name, int age) : IFormatReader
{
    private readonly Queue<(string Name, ValueKind Kind, object Value)> _members = new(
        [
            ("name", ValueKind.String, name),
            ("age", ValueKind.Number, (long)age),
        ]);
    private (string Name, ValueKind Kind, object Value)? _current;

    public ValueKind ValueKind => _current?.Kind ?? ValueKind.Null;

    public void ReadStartObject() { }

    public void ReadEndObject() { }

    public void ReadStartArray() { }    

    public void ReadEndArray() { }

    public bool TryReadPropertyName(out string name)
    {
        if (_members.Count == 0)
        {
            _current = null;
            name = string.Empty;
            return false;
        }

        _current = _members.Dequeue();
        name = _current.Value.Name;
        return true;
    }

    public bool IsNull() => false;

    public bool GetBoolean() => (bool)_current!.Value.Value;

    public long GetInt64() => (long)_current!.Value.Value;

    public double GetDouble() => (double)_current!.Value.Value;

    public string GetString() => (string)_current!.Value.Value;

    public ReadOnlySpan<byte> GetRawStringBytes() => System.Text.Encoding.UTF8.GetBytes(GetString());

    public ReadOnlySpan<byte> GetRawNumberBytes() => BitConverter.GetBytes(GetInt64());
}
