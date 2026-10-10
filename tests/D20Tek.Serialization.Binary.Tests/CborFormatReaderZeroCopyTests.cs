using D20Tek.Serialization.Binary.Cbor;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
public sealed class CborFormatReaderZeroCopyTests
{
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
