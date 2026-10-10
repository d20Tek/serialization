using D20Tek.Serialization.Dom;
using System.Diagnostics.CodeAnalysis;
using static D20Tek.Serialization.Binary.Tests.BinaryElementTestSupport;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
public sealed class BinaryElementEnumerationTests
{
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
        Assert.IsEmpty(properties);
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
        Assert.IsEmpty(items);
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
}
