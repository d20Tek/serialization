using D20Tek.Serialization.Binary.Cbor;
using D20Tek.Serialization.Dom;
using System.Diagnostics.CodeAnalysis;
using System.Formats.Cbor;

namespace D20Tek.Serialization.Binary.Tests.Cbor;

[TestClass]
public sealed class CborNodeParserTests
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

    // --- Object (map) ---

    [TestMethod]
    public void Parse_EmptyMap_ReturnsEmptyObjectNode()
    {
        // arrange
        var bytes = Encode(w =>
        {
            w.WriteStartMap(0);
            w.WriteEndMap();
        });

        // act
        var node = CborNodeParser.Parse(bytes);

        // assert
        Assert.AreEqual(NodeKind.Object, node.Kind);
        Assert.HasCount(0, node.GetObject());
    }

    [TestMethod]
    public void Parse_Map_ReturnsObjectNodeWithProperties()
    {
        // arrange
        var bytes = Encode(w =>
        {
            w.WriteStartMap(2);
            w.WriteTextString("Name");
            w.WriteTextString("Alice");
            w.WriteTextString("Age");
            w.WriteInt64(30);
            w.WriteEndMap();
        });

        // act
        var node = CborNodeParser.Parse(bytes);

        // assert
        Assert.AreEqual(NodeKind.Object, node.Kind);
        var props = node.GetObject();
        Assert.HasCount(2, props);
        Assert.AreEqual("Name", props[0].Name);
        Assert.AreEqual("Alice", props[0].Value.GetString());
        Assert.AreEqual("Age", props[1].Name);
        Assert.AreEqual(30.0, props[1].Value.GetDouble());
    }

    [TestMethod]
    public void Parse_Map_IndexerAccessByName()
    {
        // arrange
        var bytes = Encode(w =>
        {
            w.WriteStartMap(1);
            w.WriteTextString("key");
            w.WriteBoolean(true);
            w.WriteEndMap();
        });

        // act
        var node = CborNodeParser.Parse(bytes);

        // assert
        Assert.IsTrue(node["key"].GetBoolean());
    }

    // --- Array ---

    [TestMethod]
    public void Parse_EmptyArray_ReturnsEmptyArrayNode()
    {
        // arrange
        var bytes = Encode(w =>
        {
            w.WriteStartArray(0);
            w.WriteEndArray();
        });

        // act
        var node = CborNodeParser.Parse(bytes);

        // assert
        Assert.AreEqual(NodeKind.Array, node.Kind);
        Assert.HasCount(0, node.GetArray());
    }

    [TestMethod]
    public void Parse_Array_ReturnsArrayNodeWithElements()
    {
        // arrange
        var bytes = Encode(w =>
        {
            w.WriteStartArray(3);
            w.WriteInt64(1);
            w.WriteInt64(2);
            w.WriteInt64(3);
            w.WriteEndArray();
        });

        // act
        var node = CborNodeParser.Parse(bytes);

        // assert
        Assert.AreEqual(NodeKind.Array, node.Kind);
        var items = node.GetArray();
        Assert.HasCount(3, items);
        Assert.AreEqual(1.0, items[0].GetDouble());
        Assert.AreEqual(2.0, items[1].GetDouble());
        Assert.AreEqual(3.0, items[2].GetDouble());
    }

    [TestMethod]
    public void Parse_Array_IndexerAccessByIndex()
    {
        // arrange
        var bytes = Encode(w =>
        {
            w.WriteStartArray(2);
            w.WriteTextString("first");
            w.WriteTextString("second");
            w.WriteEndArray();
        });

        // act
        var node = CborNodeParser.Parse(bytes);

        // assert
        Assert.AreEqual("first", node[0].GetString());
        Assert.AreEqual("second", node[1].GetString());
    }

    // --- Nested structures ---

    [TestMethod]
    public void Parse_NestedObjectAndArray_NavigatesCorrectly()
    {
        // arrange — { "items": [1, 2], "nested": { "flag": true } }
        var bytes = Encode(w =>
        {
            w.WriteStartMap(2);
            w.WriteTextString("items");
            w.WriteStartArray(2);
            w.WriteInt64(1);
            w.WriteInt64(2);
            w.WriteEndArray();
            w.WriteTextString("nested");
            w.WriteStartMap(1);
            w.WriteTextString("flag");
            w.WriteBoolean(true);
            w.WriteEndMap();
            w.WriteEndMap();
        });

        // act
        var node = CborNodeParser.Parse(bytes);

        // assert
        Assert.AreEqual(NodeKind.Object, node.Kind);
        Assert.AreEqual(1.0, node["items"][0].GetDouble());
        Assert.AreEqual(2.0, node["items"][1].GetDouble());
        Assert.IsTrue(node["nested"]["flag"].GetBoolean());
    }

    // --- Tagged values: Guid (Tag 37) ---

    [TestMethod]
    public void Parse_GuidTag_ReturnsStringNodeWithGuidValue()
    {
        // arrange
        var guid = Guid.Parse("01020304-0506-0708-090a-0b0c0d0e0f10");
        var bytes = Encode(w =>
        {
            w.WriteTag((CborTag)37);
            Span<byte> guidBytes = stackalloc byte[16];
            guid.TryWriteBytes(guidBytes, bigEndian: true, out _);
            w.WriteByteString(guidBytes);
        });

        // act
        var node = CborNodeParser.Parse(bytes);

        // assert
        Assert.AreEqual(NodeKind.String, node.Kind);
        Assert.AreEqual(guid.ToString(), node.GetString());
    }

    // --- Tagged values: DateTime (Tag 1) ---

    [TestMethod]
    public void Parse_DateTimeTag_ReturnsStringNodeWithDateTimeValue()
    {
        // arrange
        var dateTime = new DateTime(2025, 6, 15, 12, 30, 0, DateTimeKind.Utc);
        var epochMs = (long)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        var bytes = Encode(w =>
        {
            w.WriteTag((CborTag)1);
            w.WriteInt64(epochMs);
        });

        // act
        var node = CborNodeParser.Parse(bytes);

        // assert
        Assert.AreEqual(NodeKind.String, node.Kind);
        Assert.AreEqual(dateTime.ToString("O"), node.GetString());
    }

    // --- Unknown tag: consumed and data item parsed normally ---

    [TestMethod]
    public void Parse_UnknownTag_ParsesDataItemNormally()
    {
        // arrange — Tag 99 + integer 42
        var bytes = Encode(w =>
        {
            w.WriteTag((CborTag)99);
            w.WriteInt64(42);
        });

        // act
        var node = CborNodeParser.Parse(bytes);

        // assert — the tag is consumed, the data item is a number
        Assert.AreEqual(NodeKind.Number, node.Kind);
        Assert.AreEqual(42.0, node.GetDouble());
    }

    // --- Tagged value inside an object ---

    [TestMethod]
    public void Parse_ObjectWithTaggedGuidProperty()
    {
        // arrange — { "id": Tag 37 + 16-byte bstr }
        var guid = Guid.Parse("aabbccdd-eeff-0011-2233-445566778899");
        var bytes = Encode(w =>
        {
            w.WriteStartMap(1);
            w.WriteTextString("id");
            w.WriteTag((CborTag)37);
            Span<byte> guidBytes = stackalloc byte[16];
            guid.TryWriteBytes(guidBytes, bigEndian: true, out _);
            w.WriteByteString(guidBytes);
            w.WriteEndMap();
        });

        // act
        var node = CborNodeParser.Parse(bytes);

        // assert
        Assert.AreEqual(guid.ToString(), node["id"].GetString());
    }

    // --- Indefinite-length rejection ---

    [TestMethod]
    public void Parse_IndefiniteLengthMap_Throws()
    {
        // arrange — raw CBOR: 0xBF (indefinite-length map start) + 0xFF (break)
        byte[] bytes = [0xBF, 0xFF];

        // act & assert
        Assert.ThrowsExactly<InvalidOperationException>(
            [ExcludeFromCodeCoverage] () => CborNodeParser.Parse(bytes));
    }

    [TestMethod]
    public void Parse_IndefiniteLengthArray_Throws()
    {
        // arrange — raw CBOR: 0x9F (indefinite-length array start) + 0xFF (break)
        byte[] bytes = [0x9F, 0xFF];

        // act & assert
        Assert.ThrowsExactly<InvalidOperationException>(
            [ExcludeFromCodeCoverage] () => CborNodeParser.Parse(bytes));
    }

    // --- SinglePrecisionFloat (raw CBOR) ---

    [TestMethod]
    public void Parse_SinglePrecisionFloat_RawCborBytes_ReturnsNumberNode()
    {
        // arrange — raw CBOR: 0xFA (single-precision float) + big-endian IEEE 754 for 3.14f
        byte[] bytes = [0xFA, 0x40, 0x48, 0xF5, 0xC3];

        // act
        var node = CborNodeParser.Parse(bytes);

        // assert
        Assert.AreEqual(NodeKind.Number, node.Kind);
        Assert.AreEqual(3.14f, (float)node.GetDouble(), 0.001f);
    }

    // --- Unexpected state rejection ---

    [TestMethod]
    public void Parse_UndefinedSimpleValue_ThrowsForUnexpectedState()
    {
        // arrange — raw CBOR: 0xF7 (undefined simple value, not handled in ReadNode switch)
        byte[] bytes = [0xF7];

        // act & assert
        Assert.ThrowsExactly<InvalidOperationException>(
            [ExcludeFromCodeCoverage] () => CborNodeParser.Parse(bytes));
    }

    // --- Helper ---

    private static byte[] Encode(Action<CborWriter> write)
    {
        var writer = new CborWriter(CborConformanceMode.Lax, convertIndefiniteLengthEncodings: true);
        write(writer);
        return writer.Encode();
    }
}
