using D20Tek.Serialization;
using D20Tek.Serialization.Binary.Cbor;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
public sealed class CborFormatReaderTests
{
    // --- Construction & decoding mode ---

    [TestMethod]
    public void Constructor_DefaultsToLenientDecoding()
    {
        // arrange / act
        var reader = ReaderFromHex("00");

        // assert
        Assert.AreEqual(BinaryDecodingMode.Lenient, reader.DecodingMode);
    }

    [TestMethod]
    public void Constructor_UsesSpecifiedDecodingMode()
    {
        // arrange / act
        var reader = new CborFormatReader(Convert.FromHexString("00"), BinaryDecodingMode.Strict);

        // assert
        Assert.AreEqual(BinaryDecodingMode.Strict, reader.DecodingMode);
    }

    // --- ValueKind mapping ---

    [TestMethod]
    [DataRow("F6", ValueKind.Null)]      // simple value 22 (null)
    [DataRow("F5", ValueKind.Boolean)]   // true
    [DataRow("F4", ValueKind.Boolean)]   // false
    [DataRow("00", ValueKind.Number)]    // uint 0
    [DataRow("20", ValueKind.Number)]    // negative int -1
    [DataRow("1903E8", ValueKind.Number)]            // uint 1000
    [DataRow("FB3FF8000000000000", ValueKind.Number)] // double 1.5
    [DataRow("60", ValueKind.String)]    // empty text string
    [DataRow("6449455446", ValueKind.String)]        // "IETF"
    [DataRow("40", ValueKind.String)]    // empty byte string
    [DataRow("A0", ValueKind.Object)]    // empty map
    [DataRow("80", ValueKind.Array)]     // empty array
    public void ValueKind_MapsCborStateToValueKind(string hex, ValueKind expected)
    {
        // arrange
        var reader = ReaderFromHex(hex);

        // act / assert
        Assert.AreEqual(expected, reader.ValueKind);
    }

    // --- Scalar getters ---

    [TestMethod]
    public void IsNull_OnNull_ReturnsTrueAndConsumes()
    {
        // arrange
        var reader = ReaderFromHex("F6"); // null

        // act / assert
        Assert.IsTrue(reader.IsNull());
        Assert.AreEqual(ValueKind.Null, reader.ValueKind); // nothing left to read
    }

    [TestMethod]
    public void IsNull_OnNonNull_ReturnsFalseAndDoesNotConsume()
    {
        // arrange
        var reader = ReaderFromHex("F5"); // true

        // act / assert
        Assert.IsFalse(reader.IsNull());
        Assert.IsTrue(reader.GetBoolean());
    }

    [TestMethod]
    [DataRow("F5", true)]
    [DataRow("F4", false)]
    public void GetBoolean_ReadsCborBool(string hex, bool expected)
    {
        // arrange
        var reader = ReaderFromHex(hex);

        // act / assert
        Assert.AreEqual(expected, reader.GetBoolean());
    }

    [TestMethod]
    [DataRow("00", 0L)]
    [DataRow("01", 1L)]
    [DataRow("17", 23L)]
    [DataRow("1818", 24L)]
    [DataRow("1903E8", 1000L)]
    [DataRow("20", -1L)]
    [DataRow("3863", -100L)]
    public void GetInt64_ReadsCborInteger(string hex, long expected)
    {
        // arrange
        var reader = ReaderFromHex(hex);

        // act / assert
        Assert.AreEqual(expected, reader.GetInt64());
    }

    [TestMethod]
    [DataRow("FB3FF8000000000000", 1.5)]  // double 1.5
    [DataRow("FB0000000000000000", 0.0)]  // double 0.0
    [DataRow("FBC010666666666666", -4.1)] // double -4.1
    [DataRow("00", 0.0)]                   // integer read as double
    [DataRow("1903E8", 1000.0)]            // integer 1000 read as double
    [DataRow("F93C00", 1.0)]               // half-precision 1.0
    [DataRow("FA3FC00000", 1.5)]           // single-precision 1.5
    public void GetDouble_ReadsAndWidensNumbers(string hex, double expected)
    {
        // arrange
        var reader = ReaderFromHex(hex);

        // act / assert
        Assert.AreEqual(expected, reader.GetDouble(), 0.0);
    }

    [TestMethod]
    [DataRow("60", "")]
    [DataRow("6161", "a")]
    [DataRow("6449455446", "IETF")]
    public void GetString_ReadsCborTextString(string hex, string expected)
    {
        // arrange
        var reader = ReaderFromHex(hex);

        // act / assert
        Assert.AreEqual(expected, reader.GetString());
    }

    // --- Zero-copy getters ---

    [TestMethod]
    public void GetRawStringBytes_TextString_ReturnsUtf8PayloadWithoutHeader()
    {
        // arrange
        var reader = ReaderFromHex("6449455446"); // text(4) "IETF"

        // act
        var payload = reader.GetRawStringBytes().ToArray();

        // assert
        CollectionAssert.AreEqual("IETF"u8.ToArray(), payload);
    }

    [TestMethod]
    public void GetRawStringBytes_LongTextString_HandlesOneByteLengthHeader()
    {
        // arrange (24 chars forces a 1-byte length header: 0x78 0x18)
        var value = new string('a', 24);
        var reader = ReaderFor(w => w.WriteString(value));

        // act
        var payload = reader.GetRawStringBytes().ToArray();

        // assert
        Assert.HasCount(24, payload);
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(value), payload);
    }

    [TestMethod]
    public void GetRawStringBytes_ByteString_ReturnsRawPayload()
    {
        // arrange
        var reader = ReaderFromHex("43010203"); // bstr(3) 01 02 03

        // act
        var payload = reader.GetRawStringBytes().ToArray();

        // assert
        CollectionAssert.AreEqual(new byte[] { 0x01, 0x02, 0x03 }, payload);
    }

    [TestMethod]
    public void GetRawStringBytes_OnNumber_Throws()
    {
        // arrange
        var reader = ReaderFromHex("00");

        // act / assert
        Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() => { reader.GetRawStringBytes(); });
    }

    [TestMethod]
    [DataRow("01", new byte[] { 0x01 })]                 // small integer (header is the value)
    [DataRow("1903E8", new byte[] { 0x19, 0x03, 0xE8 })] // uint 1000
    public void GetRawNumberBytes_ReturnsCompleteEncoding(string hex, byte[] expected)
    {
        // arrange
        var reader = ReaderFromHex(hex);

        // act
        var payload = reader.GetRawNumberBytes().ToArray();

        // assert
        CollectionAssert.AreEqual(expected, payload);
    }

    [TestMethod]
    public void GetRawNumberBytes_Double_ReturnsFullNineByteEncoding()
    {
        // arrange
        var reader = ReaderFromHex("FB3FF8000000000000"); // double 1.5

        // act
        var payload = reader.GetRawNumberBytes().ToArray();

        // assert
        CollectionAssert.AreEqual(Convert.FromHexString("FB3FF8000000000000"), payload);
    }

    [TestMethod]
    public void GetRawNumberBytes_OnString_Throws()
    {
        // arrange
        var reader = ReaderFromHex("6449455446"); // "IETF"

        // act / assert
        Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() => { reader.GetRawNumberBytes(); });
    }

    // --- Structural navigation ---

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

    // --- Error paths & path tracking ---

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

    // --- Indefinite-length rejection (v1 invariant) ---

    [TestMethod]
    public void ReadStartObject_IndefiniteLengthMap_Throws()
    {
        // arrange
        var reader = ReaderFromHex("BFFF"); // indefinite-length map (empty)

        // act
        var ex = Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() => reader.ReadStartObject());

        // assert
        Assert.Contains("Indefinite-length map", ex.Message);
    }

    [TestMethod]
    public void ReadStartArray_IndefiniteLengthArray_Throws()
    {
        // arrange
        var reader = ReaderFromHex("9FFF"); // indefinite-length array (empty)

        // act
        var ex = Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() => reader.ReadStartArray());

        // assert
        Assert.Contains("Indefinite-length array", ex.Message);
    }

    [TestMethod]
    public void GetString_IndefiniteLengthTextString_Throws()
    {
        // arrange
        var reader = ReaderFromHex("7FFF"); // indefinite-length text string

        // act
        var ex = Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() => reader.GetString());

        // assert
        Assert.Contains("Indefinite-length text string", ex.Message);
    }

    [TestMethod]
    public void GetRawStringBytes_IndefiniteLengthTextString_Throws()
    {
        // arrange
        var reader = ReaderFromHex("7FFF"); // indefinite-length text string

        // act
        var ex = Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() => { reader.GetRawStringBytes(); });

        // assert
        Assert.Contains("Indefinite-length string", ex.Message);
    }

    [TestMethod]
    public void GetRawStringBytes_IndefiniteLengthByteString_Throws()
    {
        // arrange
        var reader = ReaderFromHex("5FFF"); // indefinite-length byte string

        // act
        var ex = Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() => { reader.GetRawStringBytes(); });

        // assert
        Assert.Contains("Indefinite-length string", ex.Message);
    }

    // --- SkipValue: strict vs lenient ---

    [TestMethod]
    public void SkipValue_LenientMode_SkipsTaggedValueAndAdvances()
    {
        // arrange (array of [ tag(1) 0, 5 ]; the tagged item is unknown)
        var reader = ReaderFromHex("82C10005");
        reader.ReadStartArray();

        // act
        reader.SkipValue(); // skip the tagged value in lenient mode

        // assert
        Assert.AreEqual(5L, reader.GetInt64());
        reader.ReadEndArray();
    }

    [TestMethod]
    public void SkipValue_StrictMode_ThrowsOnUnknownTag()
    {
        // arrange
        var reader = new CborFormatReader(Convert.FromHexString("C100"), BinaryDecodingMode.Strict); // tag(1) 0

        // act
        var ex = Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() => reader.SkipValue());

        // assert
        Assert.Contains("Unknown tag '1'", ex.Message);
    }

    [TestMethod]
    public void SkipValue_SkipsCompositeValueAndAdvances()
    {
        // arrange (array of [ [1,2,3], 5 ])
        var reader = ReaderFromHex("828301020305");
        reader.ReadStartArray();

        // act
        reader.SkipValue(); // skip the nested array

        // assert
        Assert.AreEqual(5L, reader.GetInt64());
        reader.ReadEndArray();
    }

    // --- GetStringPayloadRange: length-header decoding (all switch branches) ---

    [TestMethod]
    [DataRow("60", 1, 0)]                  // text(0): additionalInfo <= 23, length in header
    [DataRow("77", 1, 23)]                 // text(23): largest single-byte header form
    [DataRow("40", 1, 0)]                  // byte(0): low 5 bits are major-type independent
    [DataRow("7818", 2, 24)]               // additionalInfo 24: one-byte length follows (24)
    [DataRow("78FF", 2, 255)]              // additionalInfo 24: one-byte length follows (255)
    [DataRow("790100", 3, 256)]            // additionalInfo 25: two-byte length follows (256)
    [DataRow("79FFFF", 3, 65535)]          // additionalInfo 25: two-byte length follows (65535)
    [DataRow("7A00010000", 5, 65536)]      // additionalInfo 26: four-byte length follows (65536)
    [DataRow("7B0000000000000001", 9, 1)]  // additionalInfo 27: eight-byte length follows (1)
    public void GetStringPayloadRange_DecodesEachHeaderForm(string hex, int expectedOffset, int expectedLength)
    {
        // arrange
        var encoded = Convert.FromHexString(hex);

        // act
        var (offset, length) = CborLengthDecoder.GetStringPayloadRange(encoded);

        // assert
        Assert.AreEqual(expectedOffset, offset);
        Assert.AreEqual(expectedLength, length);
    }

    [TestMethod]
    [DataRow("7C")] // additionalInfo 28 (reserved)
    [DataRow("7D")] // additionalInfo 29 (reserved)
    [DataRow("7E")] // additionalInfo 30 (reserved)
    [DataRow("7F")] // additionalInfo 31 (indefinite-length marker)
    public void GetStringPayloadRange_MalformedHeader_Throws(string hex)
    {
        // arrange
        var encoded = Convert.FromHexString(hex);

        // act
        var ex = Assert.ThrowsExactly<SerializationException>(
            [ExcludeFromCodeCoverage]() => { CborLengthDecoder.GetStringPayloadRange(encoded); });

        // assert
        Assert.Contains("Malformed CBOR string length header", ex.Message);
        Assert.AreEqual("$", ex.Path);
    }

    private static CborFormatReader ReaderFromHex(string hex) => new(Convert.FromHexString(hex));

    private static CborFormatReader ReaderFor(Action<CborFormatWriter> write)
    {
        var writer = new CborFormatWriter();
        write(writer);
        return new CborFormatReader(writer.Encode());
    }
}
