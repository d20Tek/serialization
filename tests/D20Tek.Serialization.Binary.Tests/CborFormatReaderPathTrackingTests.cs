using D20Tek.Serialization.Binary.Cbor;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
public sealed class CborFormatReaderPathTrackingTests
{
    [TestMethod]
    public void GetString_OnNumber_ThrowsWithPathAndKinds()
    {
        // arrange
        var reader = ReaderFromHex("00"); // number 0

        // act
        var ex = Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() => reader.GetString());

        // assert
        Assert.AreEqual("$", ex.Path);
        Assert.AreEqual(ValueKind.String, ex.Expected);
        Assert.AreEqual(ValueKind.Number, ex.Actual);
    }

    [TestMethod]
    public void GetInt64_OnString_ThrowsWithKinds()
    {
        // arrange
        var reader = ReaderFromHex("6449455446"); // "IETF"

        // act
        var ex = Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() => reader.GetInt64());

        // assert
        Assert.AreEqual(ValueKind.Number, ex.Expected);
        Assert.AreEqual(ValueKind.String, ex.Actual);
    }

    [TestMethod]
    public void GetDouble_OnString_ThrowsWithKinds()
    {
        // arrange
        var reader = ReaderFromHex("6449455446"); // "IETF"

        // act
        var ex = Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() => reader.GetDouble());

        // assert
        Assert.AreEqual(ValueKind.Number, ex.Expected);
        Assert.AreEqual(ValueKind.String, ex.Actual);
    }

    [TestMethod]
    public void GetBoolean_OnNumber_ThrowsWithKinds()
    {
        // arrange
        var reader = ReaderFromHex("00");

        // act
        var ex = Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() => reader.GetBoolean());

        // assert
        Assert.AreEqual(ValueKind.Boolean, ex.Expected);
        Assert.AreEqual(ValueKind.Number, ex.Actual);
    }

    [TestMethod]
    public void ReadStartObject_OnNonObject_ThrowsWithKinds()
    {
        // arrange
        var reader = ReaderFromHex("80"); // array

        // act
        var ex = Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() => reader.ReadStartObject());

        // assert
        Assert.AreEqual(ValueKind.Object, ex.Expected);
        Assert.AreEqual(ValueKind.Array, ex.Actual);
    }

    [TestMethod]
    public void ReadStartArray_OnNonArray_ThrowsWithKinds()
    {
        // arrange
        var reader = ReaderFromHex("A0"); // map

        // act
        var ex = Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() => reader.ReadStartArray());

        // assert
        Assert.AreEqual(ValueKind.Array, ex.Expected);
        Assert.AreEqual(ValueKind.Object, ex.Actual);
    }

    [TestMethod]
    public void ArrayElementPath_TracksCurrentIndex()
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
        reader.ReadStartArray();
        reader.GetInt64();
        reader.GetInt64();

        // act (positioned on the third element, index 2)
        var ex = Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() => reader.GetString());

        // assert
        Assert.AreEqual("$[2]", ex.Path);
    }

    [TestMethod]
    public void NestedPath_TracksPropertyAndIndexSegments()
    {
        // arrange
        var reader = ReaderFor(w =>
        {
            w.WriteStartObject();
            w.WritePropertyName("items");
            w.WriteStartArray();
            w.WriteStartObject();
            w.WritePropertyName("price");
            w.WriteString("abc");
            w.WriteEndObject();
            w.WriteEndArray();
            w.WriteEndObject();
        });
        reader.ReadStartObject();
        reader.TryReadPropertyName(out _); // items
        reader.ReadStartArray();
        reader.ReadStartObject();
        reader.TryReadPropertyName(out _); // price

        // act (price holds a string but we ask for a number)
        var ex = Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() => reader.GetInt64());

        // assert
        Assert.AreEqual("$.items[0].price", ex.Path);
        Assert.AreEqual(ValueKind.Number, ex.Expected);
        Assert.AreEqual(ValueKind.String, ex.Actual);
    }

    [TestMethod]
    public void PropertyPath_ReplacesPreviousSiblingProperty()
    {
        // arrange
        var reader = ReaderFor(w =>
        {
            w.WriteStartObject();
            w.WritePropertyName("a");
            w.WriteNumber(1L);
            w.WritePropertyName("b");
            w.WriteString("x");
            w.WriteEndObject();
        });
        reader.ReadStartObject();
        reader.TryReadPropertyName(out _); // a
        reader.GetInt64();                 // 1
        reader.TryReadPropertyName(out _); // b

        // act (b holds a string but we ask for a number)
        var ex = Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() => reader.GetInt64());

        // assert
        Assert.AreEqual("$.b", ex.Path);
    }

    [TestMethod]
    public void PushPropertySegment_FirstPropertyInScope_SetsPathWithoutPopping()
    {
        // arrange — covers: scope exists, HasChild is false (first property read)
        var reader = ReaderFor(w =>
        {
            w.WriteStartObject();
            w.WritePropertyName("first");
            w.WriteString("bad");
            w.WriteEndObject();
        });
        reader.ReadStartObject();
        reader.TryReadPropertyName(out _); // first

        // act — force an error to inspect the path
        var ex = Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() => reader.GetInt64());

        // assert — the path should have the single property, not a stale predecessor
        Assert.AreEqual("$.first", ex.Path);
    }

    [TestMethod]
    public void PushPropertySegment_ThirdProperty_PopsSecondBeforePushingThird()
    {
        // arrange — covers: scope.HasChild == true on subsequent (third) property
        var reader = ReaderFor(w =>
        {
            w.WriteStartObject();
            w.WritePropertyName("a");
            w.WriteNumber(1L);
            w.WritePropertyName("b");
            w.WriteNumber(2L);
            w.WritePropertyName("c");
            w.WriteString("bad");
            w.WriteEndObject();
        });
        reader.ReadStartObject();
        reader.TryReadPropertyName(out _); // a
        reader.GetInt64();
        reader.TryReadPropertyName(out _); // b
        reader.GetInt64();
        reader.TryReadPropertyName(out _); // c

        // act
        var ex = Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() => reader.GetInt64());

        // assert — path should be $.c, not $.a.b.c (proves pop happened each time)
        Assert.AreEqual("$.c", ex.Path);
    }

    [TestMethod]
    public void PushPropertySegment_NoEnclosingScope_PushesPropertyWithoutScopeTracking()
    {
        // arrange — covers: _scopes.Count == 0 (scope is null)
        // Position the reader on a bare text string without calling ReadStartObject,
        // so no scope is pushed onto _scopes.
        var reader = ReaderFromHex("6161"); // text string "a"

        // act — TryReadPropertyName succeeds because PeekState is TextString (not EndMap)
        var result = reader.TryReadPropertyName(out var name);

        // assert — property was read; path includes the segment even without a scope
        Assert.IsTrue(result);
        Assert.AreEqual("a", name);
    }

    private static CborFormatReader ReaderFromHex(string hex) => new(Convert.FromHexString(hex));

    private static CborFormatReader ReaderFor(Action<CborFormatWriter> write)
    {
        var writer = new CborFormatWriter();
        write(writer);
        return new CborFormatReader(writer.Encode());
    }
}
