using D20Tek.Serialization.Generation;

namespace D20Tek.Serialization.Generator.Tests;

[TestClass]
public sealed class SourceWriterTests
{
    [TestMethod]
    public void Line_WithText_AppendsTextAndNewline()
    {
        // arrange
        var writer = new SourceWriter();

        // act
        writer.Line("hello");

        // assert
        Assert.AreEqual("hello\n", writer.ToString());
    }

    [TestMethod]
    public void Line_WithoutText_AppendsBlankLine()
    {
        // arrange
        var writer = new SourceWriter();

        // act
        writer.Line();

        // assert
        Assert.AreEqual("\n", writer.ToString());
    }

    [TestMethod]
    public void Line_EmptyString_AppendsBlankLineWithoutIndentation()
    {
        // arrange
        var writer = new SourceWriter();
        writer.Indent();

        // act
        writer.Line(string.Empty);

        // assert
        Assert.AreEqual("\n", writer.ToString());
    }

    [TestMethod]
    public void OpenBrace_WritesBraceAndIncreasesIndent()
    {
        // arrange
        var writer = new SourceWriter();

        // act
        writer.OpenBrace();
        writer.Line("body");

        // assert
        Assert.AreEqual("{\n    body\n", writer.ToString());
    }

    [TestMethod]
    public void CloseBrace_DecreasesIndentAndWritesBrace()
    {
        // arrange
        var writer = new SourceWriter();
        writer.OpenBrace();
        writer.Line("body");

        // act
        writer.CloseBrace();

        // assert
        Assert.AreEqual("{\n    body\n}\n", writer.ToString());
    }

    [TestMethod]
    public void CloseBrace_WithSuffix_AppendsSuffix()
    {
        // arrange
        var writer = new SourceWriter();
        writer.OpenBrace();

        // act
        writer.CloseBrace(";");

        // assert
        Assert.AreEqual("{\n};\n", writer.ToString());
    }

    [TestMethod]
    public void Indent_IncreasesIndentationForSubsequentLines()
    {
        // arrange
        var writer = new SourceWriter();

        // act
        writer.Indent();
        writer.Line("indented");

        // assert
        Assert.AreEqual("    indented\n", writer.ToString());
    }

    [TestMethod]
    public void Outdent_DecreasesIndentationForSubsequentLines()
    {
        // arrange
        var writer = new SourceWriter();
        writer.Indent();
        writer.Indent();

        // act
        writer.Outdent();
        writer.Line("once");

        // assert
        Assert.AreEqual("    once\n", writer.ToString());
    }

    [TestMethod]
    public void NestedBraces_ProduceCumulativeIndentation()
    {
        // arrange
        var writer = new SourceWriter();

        // act
        writer.OpenBrace();
        writer.OpenBrace();
        writer.Line("deep");
        writer.CloseBrace();
        writer.CloseBrace();

        // assert
        Assert.AreEqual("{\n    {\n        deep\n    }\n}\n", writer.ToString());
    }

    [TestMethod]
    public void FluentMethods_ReturnSameInstance()
    {
        // arrange
        var writer = new SourceWriter();

        // act
        var afterLine = writer.Line("x");
        var afterOpen = writer.OpenBrace();
        var afterClose = writer.CloseBrace();
        var afterIndent = writer.Indent();
        var afterOutdent = writer.Outdent();

        // assert
        Assert.AreSame(writer, afterLine);
        Assert.AreSame(writer, afterOpen);
        Assert.AreSame(writer, afterClose);
        Assert.AreSame(writer, afterIndent);
        Assert.AreSame(writer, afterOutdent);
    }

    [TestMethod]
    public void ToString_EmptyWriter_ReturnsEmptyString()
    {
        // arrange
        var writer = new SourceWriter();

        // act
        var result = writer.ToString();

        // assert
        Assert.AreEqual(string.Empty, result);
    }
}
