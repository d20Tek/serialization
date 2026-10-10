using D20Tek.Serialization.Binary.Cbor;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
public sealed class CborFormatReaderScalarTests
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

    private static CborFormatReader ReaderFromHex(string hex) => new(Convert.FromHexString(hex));
}
