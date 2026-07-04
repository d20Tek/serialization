using D20Tek.Serialization.Dom;
using System.Diagnostics.CodeAnalysis;
using System.Formats.Cbor;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
public sealed class BinaryDocumentTests
{
    // --- Parse ---

    [TestMethod]
    public void Parse_ValidCborScalar_ReturnsBinaryDocument()
    {
        // arrange
        var bytes = Encode(w => w.WriteInt64(42));

        // act
        using var doc = BinaryDocument.Parse(bytes);

        // assert
        Assert.IsNotNull(doc);
        Assert.AreEqual(NodeKind.Number, doc.RootElement.ValueKind);
        Assert.AreEqual(42, doc.RootElement.GetInt32());
    }

    [TestMethod]
    public void Parse_ValidCborObject_ReturnsBinaryDocumentWithObjectRoot()
    {
        // arrange
        var bytes = Encode(w =>
        {
            w.WriteStartMap(2);
            w.WriteTextString("name");
            w.WriteTextString("Alice");
            w.WriteTextString("age");
            w.WriteInt64(30);
            w.WriteEndMap();
        });

        // act
        using var doc = BinaryDocument.Parse(bytes);

        // assert
        Assert.AreEqual(NodeKind.Object, doc.RootElement.ValueKind);
        Assert.AreEqual("Alice", doc.RootElement["name"].GetString());
        Assert.AreEqual(30, doc.RootElement["age"].GetInt32());
    }

    [TestMethod]
    public void Parse_ValidCborArray_ReturnsBinaryDocumentWithArrayRoot()
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
        using var doc = BinaryDocument.Parse(bytes);

        // assert
        Assert.AreEqual(NodeKind.Array, doc.RootElement.ValueKind);
        Assert.AreEqual(2, doc.RootElement[1].GetInt32());
    }

    [TestMethod]
    public void Parse_NullRoot_ReturnsBinaryDocumentWithNullElement()
    {
        // arrange
        var bytes = Encode(w => w.WriteSimpleValue(CborSimpleValue.Null));

        // act
        using var doc = BinaryDocument.Parse(bytes);

        // assert
        Assert.AreEqual(NodeKind.Null, doc.RootElement.ValueKind);
    }

    [TestMethod]
    public void Parse_BooleanRoot_ReturnsBinaryDocumentWithBooleanElement()
    {
        // arrange
        var bytes = Encode(w => w.WriteBoolean(true));

        // act
        using var doc = BinaryDocument.Parse(bytes);

        // assert
        Assert.AreEqual(NodeKind.Boolean, doc.RootElement.ValueKind);
        Assert.IsTrue(doc.RootElement.GetBoolean());
    }

    [TestMethod]
    public void Parse_StringRoot_ReturnsBinaryDocumentWithStringElement()
    {
        // arrange
        var bytes = Encode(w => w.WriteTextString("world"));

        // act
        using var doc = BinaryDocument.Parse(bytes);

        // assert
        Assert.AreEqual(NodeKind.String, doc.RootElement.ValueKind);
        Assert.AreEqual("world", doc.RootElement.GetString());
    }

    // --- Dispose ---

    [TestMethod]
    public void Dispose_ThenAccessRootElement_ThrowsObjectDisposedException()
    {
        // arrange
        var bytes = Encode(w => w.WriteInt64(1));
        var doc = BinaryDocument.Parse(bytes);

        // act
        doc.Dispose();

        // assert
        Assert.ThrowsExactly<ObjectDisposedException>([ExcludeFromCodeCoverage]() => _ = doc.RootElement);
    }

    [TestMethod]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // arrange
        var bytes = Encode(w => w.WriteInt64(1));
        var doc = BinaryDocument.Parse(bytes);

        // act & assert — second dispose should be safe
        doc.Dispose();
        doc.Dispose();
    }

    [TestMethod]
    public void Dispose_AfterNavigation_ThrowsOnSubsequentAccess()
    {
        // arrange
        var bytes = Encode(w =>
        {
            w.WriteStartMap(1);
            w.WriteTextString("key");
            w.WriteInt64(99);
            w.WriteEndMap();
        });
        var doc = BinaryDocument.Parse(bytes);
        var value = doc.RootElement["key"].GetInt32(); // should work before dispose

        // act
        doc.Dispose();

        // assert
        Assert.AreEqual(99, value); // value obtained before dispose is fine
        Assert.ThrowsExactly<ObjectDisposedException>([ExcludeFromCodeCoverage] () => _ = doc.RootElement);
    }

    // --- Nested document navigation ---

    [TestMethod]
    public void Parse_NestedObjectWithArray_FullNavigation()
    {
        // arrange — { "users": [{ "name": "Alice", "active": true }, { "name": "Bob", "active": false }] }
        var bytes = Encode(w =>
        {
            w.WriteStartMap(1);
            w.WriteTextString("users");
            w.WriteStartArray(2);
            w.WriteStartMap(2);
            w.WriteTextString("name");
            w.WriteTextString("Alice");
            w.WriteTextString("active");
            w.WriteBoolean(true);
            w.WriteEndMap();
            w.WriteStartMap(2);
            w.WriteTextString("name");
            w.WriteTextString("Bob");
            w.WriteTextString("active");
            w.WriteBoolean(false);
            w.WriteEndMap();
            w.WriteEndArray();
            w.WriteEndMap();
        });

        // act
        using var doc = BinaryDocument.Parse(bytes);
        var users = doc.RootElement["users"];
        var alice = users[0];
        var bob = users[1];

        // assert
        Assert.AreEqual("Alice", alice["name"].GetString());
        Assert.IsTrue(alice["active"].GetBoolean());
        Assert.AreEqual("Bob", bob["name"].GetString());
        Assert.IsFalse(bob["active"].GetBoolean());
    }

    [TestMethod]
    public void Parse_NestedObjectWithArray_EnumerateObjectAndArray()
    {
        // arrange — { "scores": [100, 200, 300] }
        var bytes = Encode(w =>
        {
            w.WriteStartMap(1);
            w.WriteTextString("scores");
            w.WriteStartArray(3);
            w.WriteInt64(100);
            w.WriteInt64(200);
            w.WriteInt64(300);
            w.WriteEndArray();
            w.WriteEndMap();
        });

        // act
        using var doc = BinaryDocument.Parse(bytes);
        var properties = doc.RootElement.EnumerateObject().ToList();
        var scores = properties[0].Value.EnumerateArray().ToList();

        // assert
        Assert.HasCount(1, properties);
        Assert.AreEqual("scores", properties[0].Name);
        Assert.HasCount(3, scores);
        Assert.AreEqual(100, scores[0].GetInt32());
        Assert.AreEqual(200, scores[1].GetInt32());
        Assert.AreEqual(300, scores[2].GetInt32());
    }

    // --- Helper ---

    private static byte[] Encode(Action<CborWriter> write)
    {
        var writer = new CborWriter(CborConformanceMode.Lax, convertIndefiniteLengthEncodings: true);
        write(writer);
        return writer.Encode();
    }
}
