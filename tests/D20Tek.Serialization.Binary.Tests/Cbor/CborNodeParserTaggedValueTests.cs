using D20Tek.Serialization.Binary.Cbor;
using D20Tek.Serialization.Dom;
using System.Formats.Cbor;
using static D20Tek.Serialization.Binary.Tests.Cbor.CborNodeParserTestSupport;

namespace D20Tek.Serialization.Binary.Tests.Cbor;

[TestClass]
public sealed class CborNodeParserTaggedValueTests
{
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
}
