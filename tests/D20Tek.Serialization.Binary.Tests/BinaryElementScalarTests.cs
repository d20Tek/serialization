using D20Tek.Serialization.Dom;
using System.Diagnostics.CodeAnalysis;
using System.Formats.Cbor;
using static D20Tek.Serialization.Binary.Tests.BinaryElementTestSupport;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
public sealed class BinaryElementScalarTests
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
}
