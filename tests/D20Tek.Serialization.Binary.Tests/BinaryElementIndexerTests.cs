using D20Tek.Serialization.Dom;
using System.Diagnostics.CodeAnalysis;
using static D20Tek.Serialization.Binary.Tests.BinaryElementTestSupport;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
public sealed class BinaryElementIndexerTests
{
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
}
