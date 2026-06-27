using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Core.Tests.Fakes;

[ExcludeFromCodeCoverage]
internal sealed class RecordingFormatWriter : IFormatWriter
{
    public List<string> Tokens { get; } = [];

    public void WriteStartObject() => Tokens.Add("StartObject");

    public void WriteEndObject() => Tokens.Add("EndObject");

    public void WriteStartArray() => Tokens.Add("StartArray");

    public void WriteEndArray() => Tokens.Add("EndArray");

    public void WritePropertyName(string name) => Tokens.Add($"Property:{name}");

    public void WriteNull() => Tokens.Add("Null");

    public void WriteBoolean(bool value) => Tokens.Add($"Boolean:{value}");

    public void WriteNumber(long value) => Tokens.Add($"Number:{value}");

    public void WriteNumber(double value) => Tokens.Add($"Number:{value}");

    public void WriteString(string value) => Tokens.Add($"String:{value}");
}
