using D20Tek.Serialization.Binary.Cbor;
using D20Tek.Serialization.Dom;
using static D20Tek.Serialization.Binary.Tests.Cbor.CborNodeParserTestSupport;

namespace D20Tek.Serialization.Binary.Tests.Cbor;

[TestClass]
public sealed class CborNodeParserStructuralTests
{
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
}
