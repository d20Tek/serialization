using D20Tek.Serialization.Dom;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Core.Tests;

[TestClass]
public sealed class NodeTests
{
    [TestMethod]
    public void Default_IsNullKind()
    {
        // arrange

        // act
        Node node = default;

        // assert
        Assert.AreEqual(NodeKind.Null, node.Kind);
    }

    [TestMethod]
    public void Null_HasNullKind()
    {
        // arrange

        // act
        var node = Node.Null;

        // assert
        Assert.AreEqual(NodeKind.Null, node.Kind);
    }

    [TestMethod]
    public void CreateBoolean_ExposesValueAndKind()
    {
        // arrange

        // act
        var node = Node.CreateBoolean(true);

        // assert
        Assert.AreEqual(NodeKind.Boolean, node.Kind);
        Assert.IsTrue(node.GetBoolean());
    }

    [TestMethod]
    public void CreateNumber_ExposesValueAndKind()
    {
        // arrange

        // act
        var node = Node.CreateNumber(42.5);

        // assert
        Assert.AreEqual(NodeKind.Number, node.Kind);
        Assert.AreEqual(42.5, node.GetDouble());
    }

    [TestMethod]
    public void CreateString_ExposesValueAndKind()
    {
        // arrange

        // act
        var node = Node.CreateString("hello");

        // assert
        Assert.AreEqual(NodeKind.String, node.Kind);
        Assert.AreEqual("hello", node.GetString());
    }

    [TestMethod]
    public void CreateString_WithNull_Throws()
    {
        // arrange

        // act + assert
        Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage] () => Node.CreateString(null!));
    }

    [TestMethod]
    public void CreateArray_ExposesElementsAndKind()
    {
        // arrange
        var items = new[] { Node.CreateNumber(1), Node.CreateNumber(2) };

        // act
        var node = Node.CreateArray(items);

        // assert
        Assert.AreEqual(NodeKind.Array, node.Kind);
        Assert.HasCount(2, node.GetArray());
    }

    [TestMethod]
    public void CreateArray_WithNull_Throws()
    {
        // arrange

        // act + assert
        Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => Node.CreateArray(null!));
    }

    [TestMethod]
    public void CreateObject_ExposesPropertiesAndKind()
    {
        // arrange
        var properties = new[]
        {
            new NodeProperty("name", Node.CreateString("Alice")),
            new NodeProperty("age", Node.CreateNumber(30)),
        };

        // act
        var node = Node.CreateObject(properties);

        // assert
        Assert.AreEqual(NodeKind.Object, node.Kind);
        Assert.HasCount(2, node.GetObject());
    }

    [TestMethod]
    public void CreateObject_WithNull_Throws()
    {
        // arrange

        // act + assert
        Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => Node.CreateObject(null!));
    }

    [TestMethod]
    public void Indexer_ByName_ReturnsPropertyValue()
    {
        // arrange
        var node = Node.CreateObject(
        [
            new NodeProperty("name", Node.CreateString("Alice")),
            new NodeProperty("age", Node.CreateNumber(30)),
        ]);

        // act
        var name = node["name"];
        var age = node["age"];

        // assert
        Assert.AreEqual("Alice", name.GetString());
        Assert.AreEqual(30, age.GetDouble());
    }

    [TestMethod]
    public void Indexer_ByName_MissingProperty_Throws()
    {
        // arrange
        var node = Node.CreateObject([new NodeProperty("name", Node.CreateString("Alice"))]);

        // act + assert
        Assert.ThrowsExactly<KeyNotFoundException>([ExcludeFromCodeCoverage] () => _ = node["missing"]);
    }

    [TestMethod]
    public void Indexer_ByName_WithNullName_Throws()
    {
        // arrange
        var node = Node.CreateObject([new NodeProperty("name", Node.CreateString("Alice"))]);

        // act + assert
        Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage] () => _ = node[null!]);
    }

    [TestMethod]
    public void Indexer_ByName_OnNonObject_Throws()
    {
        // arrange
        var node = Node.CreateNumber(1);

        // act + assert
        Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage] () => _ = node["name"]);
    }

    [TestMethod]
    public void Indexer_ByIndex_ReturnsElement()
    {
        // arrange
        var node = Node.CreateArray([Node.CreateNumber(10), Node.CreateNumber(20)]);

        // act
        var first = node[0];
        var second = node[1];

        // assert
        Assert.AreEqual(10, first.GetDouble());
        Assert.AreEqual(20, second.GetDouble());
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(2)]
    public void Indexer_ByIndex_OutOfRange_Throws(int index)
    {
        // arrange
        var node = Node.CreateArray([Node.CreateNumber(10), Node.CreateNumber(20)]);

        // act + assert
        Assert.ThrowsExactly<ArgumentOutOfRangeException>([ExcludeFromCodeCoverage] () => _ = node[index]);
    }

    [TestMethod]
    public void Indexer_ByIndex_OnNonArray_Throws()
    {
        // arrange
        var node = Node.CreateString("not-an-array");

        // act + assert
        Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage] () => _ = node[0]);
    }

    [TestMethod]
    public void GetString_OnNonString_Throws()
    {
        // arrange
        var node = Node.CreateNumber(1);

        // act + assert
        Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage] () => node.GetString());
    }

    [TestMethod]
    public void GetDouble_OnNonNumber_Throws()
    {
        // arrange
        var node = Node.CreateString("nope");

        // act + assert
        Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage] () => node.GetDouble());
    }

    [TestMethod]
    public void GetBoolean_OnNonBoolean_Throws()
    {
        // arrange
        var node = Node.CreateNumber(0);

        // act + assert
        Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage] () => node.GetBoolean());
    }

    [TestMethod]
    public void GetArray_OnNonArray_Throws()
    {
        // arrange
        var node = Node.CreateObject([]);

        // act + assert
        Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage] () => node.GetArray());
    }

    [TestMethod]
    public void GetObject_OnNonObject_Throws()
    {
        // arrange
        var node = Node.CreateArray([]);

        // act + assert
        Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage] () => node.GetObject());
    }

    [TestMethod]
    public void NestedTree_TraversesByMixedIndexers()
    {
        // arrange
        var node = Node.CreateObject(
        [
            new NodeProperty("items", Node.CreateArray(
            [
                Node.CreateObject([new NodeProperty("price", Node.CreateNumber(9.99))]),
            ])),
        ]);

        // act
        var price = node["items"][0]["price"];

        // assert
        Assert.AreEqual(9.99, price.GetDouble());
    }
}
