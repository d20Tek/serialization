using D20Tek.Serialization.Binary.Cbor;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
public sealed class CborFormatReaderNavigationTests
{
    [TestMethod]
    public void EmptyObject_ReadsStartAndEnd()
    {
        // arrange
        var reader = ReaderFromHex("A0"); // map(0)

        // act
        Assert.AreEqual(ValueKind.Object, reader.ValueKind);
        reader.ReadStartObject();
        var hasProperty = reader.TryReadPropertyName(out var name);
        reader.ReadEndObject();

        // assert
        Assert.IsFalse(hasProperty);
        Assert.AreEqual(string.Empty, name);
    }

    [TestMethod]
    public void EmptyArray_ReadsStartAndEnd()
    {
        // arrange
        var reader = ReaderFromHex("80"); // array(0)

        // act
        Assert.AreEqual(ValueKind.Array, reader.ValueKind);
        reader.ReadStartArray();
        reader.ReadEndArray();

        // assert
        Assert.AreEqual(ValueKind.Null, reader.ValueKind); // reader is finished
    }

    [TestMethod]
    public void Object_ReadsPropertiesInOrder()
    {
        // arrange
        var reader = ReaderFor(w =>
        {
            w.WriteStartObject();
            w.WritePropertyName("a");
            w.WriteNumber(1L);
            w.WritePropertyName("b");
            w.WriteBoolean(true);
            w.WriteEndObject();
        });

        // act / assert
        reader.ReadStartObject();
        Assert.IsTrue(reader.TryReadPropertyName(out var first));
        Assert.AreEqual("a", first);
        Assert.AreEqual(1L, reader.GetInt64());
        Assert.IsTrue(reader.TryReadPropertyName(out var second));
        Assert.AreEqual("b", second);
        Assert.IsTrue(reader.GetBoolean());
        Assert.IsFalse(reader.TryReadPropertyName(out _));
        reader.ReadEndObject();
    }

    [TestMethod]
    public void Array_ReadsElementsInOrder()
    {
        // arrange
        var reader = ReaderFor(w =>
        {
            w.WriteStartArray();
            w.WriteNumber(1L);
            w.WriteNumber(2L);
            w.WriteNumber(3L);
            w.WriteEndArray();
        });

        // act / assert
        reader.ReadStartArray();
        Assert.AreEqual(1L, reader.GetInt64());
        Assert.AreEqual(2L, reader.GetInt64());
        Assert.AreEqual(3L, reader.GetInt64());
        reader.ReadEndArray();
    }

    [TestMethod]
    public void NestedObjectAndArray_RoundTripNavigation()
    {
        // arrange
        var reader = ReaderFor(w =>
        {
            w.WriteStartObject();
            w.WritePropertyName("name");
            w.WriteString("test");
            w.WritePropertyName("values");
            w.WriteStartArray();
            w.WriteNumber(1L);
            w.WriteNumber(2L);
            w.WriteNumber(3L);
            w.WriteEndArray();
            w.WriteEndObject();
        });

        // act / assert
        reader.ReadStartObject();
        Assert.IsTrue(reader.TryReadPropertyName(out var p1));
        Assert.AreEqual("name", p1);
        Assert.AreEqual("test", reader.GetString());
        Assert.IsTrue(reader.TryReadPropertyName(out var p2));
        Assert.AreEqual("values", p2);
        reader.ReadStartArray();
        Assert.AreEqual(1L, reader.GetInt64());
        Assert.AreEqual(2L, reader.GetInt64());
        Assert.AreEqual(3L, reader.GetInt64());
        reader.ReadEndArray();
        Assert.IsFalse(reader.TryReadPropertyName(out _));
        reader.ReadEndObject();
    }

    [TestMethod]
    public void TryReadPropertyName_NonStringKey_Throws()
    {
        // arrange
        var reader = ReaderFromHex("A10001"); // map(1) { 0: 1 }
        reader.ReadStartObject();

        // act
        var ex = Assert.ThrowsExactly<SerializationException>(
            [ExcludeFromCodeCoverage]() => reader.TryReadPropertyName(out _));

        // assert
        Assert.AreEqual(ValueKind.String, ex.Expected);
        Assert.AreEqual(ValueKind.Number, ex.Actual);
    }

    [TestMethod]
    public void TryReadPropertyName_IndefiniteLengthTextStringKey_Throws()
    {
        // arrange
        // A1 = map(1), 7F = start indefinite-length text string, 63666F6F = chunk "foo", FF = break, 01 = value 1
        var reader = ReaderFromHex("A17F63666F6FFF01");
        reader.ReadStartObject();

        // act
        var ex = Assert.ThrowsExactly<SerializationException>(
            [ExcludeFromCodeCoverage]() => reader.TryReadPropertyName(out _));

        // assert
        Assert.Contains("Indefinite-length text string", ex.Message);
    }

    private static CborFormatReader ReaderFromHex(string hex) => new(Convert.FromHexString(hex));

    private static CborFormatReader ReaderFor(Action<CborFormatWriter> write)
    {
        var writer = new CborFormatWriter();
        write(writer);
        return new CborFormatReader(writer.Encode());
    }
}
