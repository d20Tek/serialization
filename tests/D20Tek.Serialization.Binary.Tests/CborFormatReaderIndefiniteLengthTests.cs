using D20Tek.Serialization.Binary.Cbor;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
public sealed class CborFormatReaderTests
{
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

    private static CborFormatReader ReaderFromHex(string hex) => new(Convert.FromHexString(hex));
}
