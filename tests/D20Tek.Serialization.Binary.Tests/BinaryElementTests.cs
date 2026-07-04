using D20Tek.Serialization.Dom;
using System.Diagnostics.CodeAnalysis;
using System.Formats.Cbor;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
public sealed class BinaryElementTests
{
    // --- ValueKind ---

    [TestMethod]
    public void ValueKind_NullElement_ReturnsNull()
    {
        // arrange
        var doc = ParseCbor(w => w.WriteSimpleValue(CborSimpleValue.Null));

        // act & assert
        Assert.AreEqual(NodeKind.Null, doc.RootElement.ValueKind);
    }

    [TestMethod]
    public void ValueKind_BooleanElement_ReturnsBoolean()
    {
        // arrange
        var doc = ParseCbor(w => w.WriteBoolean(true));

        // act & assert
        Assert.AreEqual(NodeKind.Boolean, doc.RootElement.ValueKind);
    }

    [TestMethod]
    public void ValueKind_NumberElement_ReturnsNumber()
    {
        // arrange
        var doc = ParseCbor(w => w.WriteInt64(42));

        // act & assert
        Assert.AreEqual(NodeKind.Number, doc.RootElement.ValueKind);
    }

    [TestMethod]
    public void ValueKind_StringElement_ReturnsString()
    {
        // arrange
        var doc = ParseCbor(w => w.WriteTextString("hello"));

        // act & assert
        Assert.AreEqual(NodeKind.String, doc.RootElement.ValueKind);
    }

    [TestMethod]
    public void ValueKind_ObjectElement_ReturnsObject()
    {
        // arrange
        var doc = ParseCbor(w =>
        {
            w.WriteStartMap(1);
            w.WriteTextString("key");
            w.WriteInt64(1);
            w.WriteEndMap();
        });

        // act & assert
        Assert.AreEqual(NodeKind.Object, doc.RootElement.ValueKind);
    }

    [TestMethod]
    public void ValueKind_ArrayElement_ReturnsArray()
    {
        // arrange
        var doc = ParseCbor(w =>
        {
            w.WriteStartArray(2);
            w.WriteInt64(1);
            w.WriteInt64(2);
            w.WriteEndArray();
        });

        // act & assert
        Assert.AreEqual(NodeKind.Array, doc.RootElement.ValueKind);
    }

    // --- GetBoolean ---

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void GetBoolean_BooleanElement_ReturnsValue(bool value)
    {
        // arrange
        var doc = ParseCbor(w => w.WriteBoolean(value));

        // act
        var result = doc.RootElement.GetBoolean();

        // assert
        Assert.AreEqual(value, result);
    }

    [TestMethod]
    public void GetBoolean_NonBooleanElement_Throws()
    {
        // arrange
        var doc = ParseCbor(w => w.WriteInt64(1));

        // act & assert
        Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => doc.RootElement.GetBoolean());
    }

    // --- GetString ---

    [TestMethod]
    [DataRow("")]
    [DataRow("hello")]
    public void GetString_StringElement_ReturnsValue(string value)
    {
        // arrange
        var doc = ParseCbor(w => w.WriteTextString(value));

        // act
        var result = doc.RootElement.GetString();

        // assert
        Assert.AreEqual(value, result);
    }

    [TestMethod]
    public void GetString_NonStringElement_Throws()
    {
        // arrange
        var doc = ParseCbor(w => w.WriteInt64(1));

        // act & assert
        Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => doc.RootElement.GetString());
    }

    // --- GetInt32 ---

    [TestMethod]
    public void GetInt32_NumberElement_ReturnsTruncatedValue()
    {
        // arrange
        var doc = ParseCbor(w => w.WriteInt64(42));

        // act
        var result = doc.RootElement.GetInt32();

        // assert
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void GetInt32_NonNumberElement_Throws()
    {
        // arrange
        var doc = ParseCbor(w => w.WriteBoolean(true));

        // act & assert
        Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => doc.RootElement.GetInt32());
    }

    // --- GetInt64 ---

    [TestMethod]
    public void GetInt64_NumberElement_ReturnsTruncatedValue()
    {
        // arrange
        var doc = ParseCbor(w => w.WriteInt64(1_000_000_000_000L));

        // act
        var result = doc.RootElement.GetInt64();

        // assert
        Assert.AreEqual(1_000_000_000_000L, result);
    }

    [TestMethod]
    public void GetInt64_NonNumberElement_Throws()
    {
        // arrange
        var doc = ParseCbor(w => w.WriteTextString("text"));

        // act & assert
        Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => doc.RootElement.GetInt64());
    }

    // --- GetDouble ---

    [TestMethod]
    public void GetDouble_NumberElement_ReturnsValue()
    {
        // arrange
        var doc = ParseCbor(w => w.WriteDouble(3.14));

        // act
        var result = doc.RootElement.GetDouble();

        // assert
        Assert.AreEqual(3.14, result, 0.001);
    }

    [TestMethod]
    public void GetDouble_NonNumberElement_Throws()
    {
        // arrange
        var doc = ParseCbor(w => w.WriteSimpleValue(CborSimpleValue.Null));

        // act & assert
        Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => doc.RootElement.GetDouble());
    }

    // --- String indexer ---

    [TestMethod]
    public void StringIndexer_ObjectElement_ReturnsPropertyValue()
    {
        // arrange
        var doc = ParseCbor(w =>
        {
            w.WriteStartMap(2);
            w.WriteTextString("name");
            w.WriteTextString("Alice");
            w.WriteTextString("age");
            w.WriteInt64(30);
            w.WriteEndMap();
        });

        // act
        var name = doc.RootElement["name"];
        var age = doc.RootElement["age"];

        // assert
        Assert.AreEqual(NodeKind.String, name.ValueKind);
        Assert.AreEqual("Alice", name.GetString());
        Assert.AreEqual(NodeKind.Number, age.ValueKind);
        Assert.AreEqual(30, age.GetInt32());
    }

    [TestMethod]
    public void StringIndexer_MissingProperty_ThrowsKeyNotFound()
    {
        // arrange
        var doc = ParseCbor(w =>
        {
            w.WriteStartMap(1);
            w.WriteTextString("key");
            w.WriteInt64(1);
            w.WriteEndMap();
        });

        // act & assert
        Assert.ThrowsExactly<KeyNotFoundException>([ExcludeFromCodeCoverage]() => _ = doc.RootElement["missing"]);
    }

    [TestMethod]
    public void StringIndexer_NonObjectElement_Throws()
    {
        // arrange
        var doc = ParseCbor(w => w.WriteInt64(1));

        // act & assert
        Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => _ = doc.RootElement["key"]);
    }

    // --- Int indexer ---

    [TestMethod]
    public void IntIndexer_ArrayElement_ReturnsElementAtIndex()
    {
        // arrange
        var doc = ParseCbor(w =>
        {
            w.WriteStartArray(3);
            w.WriteInt64(10);
            w.WriteInt64(20);
            w.WriteInt64(30);
            w.WriteEndArray();
        });

        // act
        var second = doc.RootElement[1];

        // assert
        Assert.AreEqual(20, second.GetInt32());
    }

    [TestMethod]
    public void IntIndexer_OutOfRange_Throws()
    {
        // arrange
        var doc = ParseCbor(w =>
        {
            w.WriteStartArray(1);
            w.WriteInt64(1);
            w.WriteEndArray();
        });

        // act & assert
        Assert.ThrowsExactly<ArgumentOutOfRangeException>([ExcludeFromCodeCoverage]() => _ = doc.RootElement[5]);
    }

    [TestMethod]
    public void IntIndexer_NegativeIndex_Throws()
    {
        // arrange
        var doc = ParseCbor(w =>
        {
            w.WriteStartArray(1);
            w.WriteInt64(1);
            w.WriteEndArray();
        });

        // act & assert
        Assert.ThrowsExactly<ArgumentOutOfRangeException>([ExcludeFromCodeCoverage]() => _ = doc.RootElement[-1]);
    }

    [TestMethod]
    public void IntIndexer_NonArrayElement_Throws()
    {
        // arrange
        var doc = ParseCbor(w => w.WriteInt64(1));

        // act & assert
        Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => _ = doc.RootElement[0]);
    }

    // --- EnumerateObject ---

    [TestMethod]
    public void EnumerateObject_ObjectElement_ReturnsAllProperties()
    {
        // arrange
        var doc = ParseCbor(w =>
        {
            w.WriteStartMap(2);
            w.WriteTextString("x");
            w.WriteInt64(1);
            w.WriteTextString("y");
            w.WriteInt64(2);
            w.WriteEndMap();
        });

        // act
        var properties = doc.RootElement.EnumerateObject().ToList();

        // assert
        Assert.HasCount(2, properties);
        Assert.AreEqual("x", properties[0].Name);
        Assert.AreEqual(1, properties[0].Value.GetInt32());
        Assert.AreEqual("y", properties[1].Name);
        Assert.AreEqual(2, properties[1].Value.GetInt32());
    }

    [TestMethod]
    public void EnumerateObject_EmptyObject_ReturnsEmpty()
    {
        // arrange
        var doc = ParseCbor(w =>
        {
            w.WriteStartMap(0);
            w.WriteEndMap();
        });

        // act
        var properties = doc.RootElement.EnumerateObject().ToList();

        // assert
        Assert.HasCount(0, properties);
    }

    [TestMethod]
    public void EnumerateObject_NonObjectElement_Throws()
    {
        // arrange
        var doc = ParseCbor(w => w.WriteInt64(1));

        // act & assert
        Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() =>
            doc.RootElement.EnumerateObject().ToList());
    }

    // --- EnumerateArray ---

    [TestMethod]
    public void EnumerateArray_ArrayElement_ReturnsAllElements()
    {
        // arrange
        var doc = ParseCbor(w =>
        {
            w.WriteStartArray(3);
            w.WriteTextString("a");
            w.WriteTextString("b");
            w.WriteTextString("c");
            w.WriteEndArray();
        });

        // act
        var items = doc.RootElement.EnumerateArray().ToList();

        // assert
        Assert.HasCount(3, items);
        Assert.AreEqual("a", items[0].GetString());
        Assert.AreEqual("b", items[1].GetString());
        Assert.AreEqual("c", items[2].GetString());
    }

    [TestMethod]
    public void EnumerateArray_EmptyArray_ReturnsEmpty()
    {
        // arrange
        var doc = ParseCbor(w =>
        {
            w.WriteStartArray(0);
            w.WriteEndArray();
        });

        // act
        var items = doc.RootElement.EnumerateArray().ToList();

        // assert
        Assert.HasCount(0, items);
    }

    [TestMethod]
    public void EnumerateArray_NonArrayElement_Throws()
    {
        // arrange
        var doc = ParseCbor(w => w.WriteInt64(1));

        // act & assert
        Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => doc.RootElement.EnumerateArray().ToList());
    }

    // --- Nested navigation ---

    [TestMethod]
    public void NestedNavigation_ObjectWithArray_TraversesCorrectly()
    {
        // arrange — { "items": [10, 20, 30] }
        var doc = ParseCbor(w =>
        {
            w.WriteStartMap(1);
            w.WriteTextString("items");
            w.WriteStartArray(3);
            w.WriteInt64(10);
            w.WriteInt64(20);
            w.WriteInt64(30);
            w.WriteEndArray();
            w.WriteEndMap();
        });

        // act
        var items = doc.RootElement["items"];

        // assert
        Assert.AreEqual(NodeKind.Array, items.ValueKind);
        Assert.AreEqual(20, items[1].GetInt32());
    }

    [TestMethod]
    public void NestedNavigation_ArrayOfObjects_TraversesCorrectly()
    {
        // arrange — [{ "name": "Alice" }, { "name": "Bob" }]
        var doc = ParseCbor(w =>
        {
            w.WriteStartArray(2);
            w.WriteStartMap(1);
            w.WriteTextString("name");
            w.WriteTextString("Alice");
            w.WriteEndMap();
            w.WriteStartMap(1);
            w.WriteTextString("name");
            w.WriteTextString("Bob");
            w.WriteEndMap();
            w.WriteEndArray();
        });

        // act
        var second = doc.RootElement[1];

        // assert
        Assert.AreEqual("Bob", second["name"].GetString());
    }

    // --- Helper ---

    private static BinaryDocument ParseCbor(Action<CborWriter> write)
    {
        var writer = new CborWriter(CborConformanceMode.Lax, convertIndefiniteLengthEncodings: true);
        write(writer);
        return BinaryDocument.Parse(writer.Encode());
    }
}
