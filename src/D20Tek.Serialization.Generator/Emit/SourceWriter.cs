using System.Text;

namespace D20Tek.Serialization.Generation;

/// <summary>
/// A minimal indentation-aware builder for emitting readable generated C# source.
/// </summary>
internal sealed class SourceWriter
{
    private readonly StringBuilder _builder = new();
    private int _indent;

    public SourceWriter Line(string text = "")
    {
        if (text.Length == 0)
        {
            _builder.Append('\n');
            return this;
        }

        _builder.Append(' ', _indent * 4).Append(text).Append('\n');
        return this;
    }

    public SourceWriter OpenBrace()
    {
        Line("{");
        _indent++;
        return this;
    }

    public SourceWriter CloseBrace(string suffix = "")
    {
        _indent--;
        Line("}" + suffix);
        return this;
    }

    public SourceWriter Indent()
    {
        _indent++;
        return this;
    }

    public SourceWriter Outdent()
    {
        _indent--;
        return this;
    }

    public override string ToString() => _builder.ToString();
}
