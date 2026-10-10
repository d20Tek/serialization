using D20Tek.Serialization.Binary.Cbor;
using D20Tek.Serialization.Dom;
using System.Formats.Cbor;
using static D20Tek.Serialization.Binary.Tests.Cbor.CborNodeParserTestSupport;

namespace D20Tek.Serialization.Binary.Tests.Cbor;

[TestClass]
public sealed class CborNodeParserScalarTests
{
    // --- Null ---

    [TestMethod]
    public void Parse_Null_ReturnsNullNode()
    {
        // arrange
        var bytes = Encode(w => w.WriteSimpleValue(CborSimpleValue.Null));

        // act
        var node = CborNodeParser.Parse(bytes);

        // assert
        Assert.AreEqual(NodeKind.Null, node.Kind);
    }

    // --- Boolean ---

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Parse_Boolean_ReturnsBooleanNode(bool value)
    {
        // arrange
        var bytes = Encode(w => w.WriteBoolean(value));

        // act
        var node = CborNodeParser.Parse(bytes);

        // assert
        Assert.AreEqual(NodeKind.Boolean, node.Kind);
        Assert.AreEqual(value, node.GetBoolean());
    }

    // --- Integer ---

    [TestMethod]
    [DataRow(0L)]
    [DataRow(1L)]
    [DataRow(-1L)]
    [DataRow(1000L)]
    [DataRow(-100L)]
    public void Parse_Integer_ReturnsNumberNode(long value)
    {
        // arrange
        var bytes = Encode(w => w.WriteInt64(value));

        // act
        var node = CborNodeParser.Parse(bytes);

        // assert
        Assert.AreEqual(NodeKind.Number, node.Kind);
        Assert.AreEqual((double)value, node.GetDouble());
    }

    // --- Float ---

    [TestMethod]
    public void Parse_Double_ReturnsNumberNode()
    {
        // arrange
        var bytes = Encode(w => w.WriteDouble(3.14));

        // act
        var node = CborNodeParser.Parse(bytes);

        // assert
        Assert.AreEqual(NodeKind.Number, node.Kind);
        Assert.AreEqual(3.14, node.GetDouble(), 0.001);
    }

    [TestMethod]
    public void Parse_Single_ReturnsNumberNode()
    {
        // arrange
        var bytes = Encode(w => w.WriteSingle(1.5f));

        // act
        var node = CborNodeParser.Parse(bytes);

        // assert
        Assert.AreEqual(NodeKind.Number, node.Kind);
        Assert.AreEqual(1.5, node.GetDouble(), 0.001);
    }

    [TestMethod]
    public void Parse_Half_ReturnsNumberNode()
    {
        // arrange
        var bytes = Encode(w => w.WriteHalf((Half)2.0));

        // act
        var node = CborNodeParser.Parse(bytes);

        // assert
        Assert.AreEqual(NodeKind.Number, node.Kind);
        Assert.AreEqual(2.0, node.GetDouble(), 0.001);
    }

    // --- String ---

    [TestMethod]
    [DataRow("")]
    [DataRow("hello")]
    [DataRow("IETF")]
    public void Parse_TextString_ReturnsStringNode(string value)
    {
        // arrange
        var bytes = Encode(w => w.WriteTextString(value));

        // act
        var node = CborNodeParser.Parse(bytes);

        // assert
        Assert.AreEqual(NodeKind.String, node.Kind);
        Assert.AreEqual(value, node.GetString());
    }

    [TestMethod]
    public void Parse_ByteString_ReturnsBase64StringNode()
    {
        // arrange
        byte[] payload = [0x01, 0x02, 0x03];
        var bytes = Encode(w => w.WriteByteString(payload));

        // act
        var node = CborNodeParser.Parse(bytes);

        // assert
        Assert.AreEqual(NodeKind.String, node.Kind);
        Assert.AreEqual(Convert.ToBase64String(payload), node.GetString());
    }
}
