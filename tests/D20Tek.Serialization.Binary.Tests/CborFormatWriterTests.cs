using D20Tek.Serialization.Binary.Cbor;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
public sealed class CborFormatWriterTests
{
    [TestMethod]
    public void WriteNull_EmitsSimpleValue22()
    {
        // arrange
        var writer = new CborFormatWriter();

        // act
        writer.WriteNull();

        // assert
        AssertHex("F6", writer);
    }

    [TestMethod]
    [DataRow(true, "F5")]
    [DataRow(false, "F4")]
    public void WriteBoolean_EmitsCborBool(bool value, string expected)
    {
        // arrange
        var writer = new CborFormatWriter();

        // act
        writer.WriteBoolean(value);

        // assert
        AssertHex(expected, writer);
    }

    [TestMethod]
    [DataRow(0L, "00")]
    [DataRow(1L, "01")]
    [DataRow(23L, "17")]
    [DataRow(24L, "1818")]
    [DataRow(100L, "1864")]
    [DataRow(1000L, "1903E8")]
    [DataRow(-1L, "20")]
    [DataRow(-100L, "3863")]
    public void WriteNumber_Integer_EmitsCborInteger(long value, string expected)
    {
        // arrange
        var writer = new CborFormatWriter();

        // act
        writer.WriteNumber(value);

        // assert
        AssertHex(expected, writer);
    }

    [TestMethod]
    [DataRow(0.0, "FB0000000000000000")]
    [DataRow(1.0, "FB3FF0000000000000")]
    [DataRow(1.5, "FB3FF8000000000000")]
    [DataRow(-4.1, "FBC010666666666666")]
    public void WriteNumber_Double_AlwaysEmits64BitIeee754(double value, string expected)
    {
        // arrange
        var writer = new CborFormatWriter();

        // act
        writer.WriteNumber(value);

        // assert
        AssertHex(expected, writer);
    }

    [TestMethod]
    [DataRow("", "60")]
    [DataRow("a", "6161")]
    [DataRow("IETF", "6449455446")]
    public void WriteString_EmitsCborTextString(string value, string expected)
    {
        // arrange
        var writer = new CborFormatWriter();

        // act
        writer.WriteString(value);

        // assert
        AssertHex(expected, writer);
    }

    [TestMethod]
    public void WriteByteString_EmitsCborByteString()
    {
        // arrange
        var writer = new CborFormatWriter();

        // act
        writer.WriteByteString([0x01, 0x02, 0x03]);

        // assert
        AssertHex("43010203", writer);
    }

    [TestMethod]
    public void WriteByteString_Empty_EmitsZeroLengthByteString()
    {
        // arrange
        var writer = new CborFormatWriter();

        // act
        writer.WriteByteString([]);

        // assert
        AssertHex("40", writer);
    }

    [TestMethod]
    public void EmptyObject_EmitsDefiniteLengthMap()
    {
        // arrange
        var writer = new CborFormatWriter();

        // act
        writer.WriteStartObject();
        writer.WriteEndObject();

        // assert
        AssertHex("A0", writer);
    }

    [TestMethod]
    public void Object_EmitsDefiniteLengthMapWithStringKeys()
    {
        // arrange
        var writer = new CborFormatWriter();

        // act
        writer.WriteStartObject();
        writer.WritePropertyName("a");
        writer.WriteNumber(1L);
        writer.WritePropertyName("b");
        writer.WriteBoolean(true);
        writer.WriteEndObject();

        // assert
        // A2 = map(2), 6161 = "a", 01 = 1, 6162 = "b", F5 = true
        AssertHex("A26161016162F5", writer);
    }

    [TestMethod]
    public void EmptyArray_EmitsDefiniteLengthArray()
    {
        // arrange
        var writer = new CborFormatWriter();

        // act
        writer.WriteStartArray();
        writer.WriteEndArray();

        // assert
        AssertHex("80", writer);
    }

    [TestMethod]
    public void Array_EmitsDefiniteLengthArray()
    {
        // arrange
        var writer = new CborFormatWriter();

        // act
        writer.WriteStartArray();
        writer.WriteNumber(1L);
        writer.WriteNumber(2L);
        writer.WriteNumber(3L);
        writer.WriteEndArray();

        // assert
        AssertHex("83010203", writer);
    }

    [TestMethod]
    public void NestedObjectAndArray_EmitsDefiniteLengthContainers()
    {
        // arrange
        var writer = new CborFormatWriter();

        // act
        writer.WriteStartObject();
        writer.WritePropertyName("items");
        writer.WriteStartArray();
        writer.WriteNumber(1L);
        writer.WriteNumber(2L);
        writer.WriteEndArray();
        writer.WriteEndObject();

        // assert
        // A1 = map(1), 656974656D73 = "items", 82 = array(2), 01 02 = 1, 2
        AssertHex("A1656974656D73820102", writer);
    }

    [TestMethod]
    public void WritePropertyName_Null_Throws()
    {
        // arrange
        var writer = new CborFormatWriter();
        writer.WriteStartObject();

        // act / assert
        Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => writer.WritePropertyName(null!));
    }

    [TestMethod]
    public void WriteString_Null_Throws()
    {
        // arrange
        var writer = new CborFormatWriter();

        // act / assert
        Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => writer.WriteString(null!));
    }

    [TestMethod]
    public void Encode_ToBufferWriter_MatchesEncodeToByteArray()
    {
        // arrange
        var writer = new CborFormatWriter();
        writer.WriteStartArray();
        writer.WriteNumber(1L);
        writer.WriteNumber(2L);
        writer.WriteEndArray();
        var expected = writer.Encode();
        var bufferWriter = new ArrayBufferWriter<byte>();

        // act
        var writer2 = new CborFormatWriter();
        writer2.WriteStartArray();
        writer2.WriteNumber(1L);
        writer2.WriteNumber(2L);
        writer2.WriteEndArray();
        writer2.Encode(bufferWriter);

        // assert
        CollectionAssert.AreEqual(expected, bufferWriter.WrittenSpan.ToArray());
    }

    [TestMethod]
    public void Encode_ToBufferWriter_Null_Throws()
    {
        // arrange
        var writer = new CborFormatWriter();
        writer.WriteNull();

        // act / assert
        Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => writer.Encode(null!));
    }

    private static void AssertHex(string expected, CborFormatWriter writer)
    {
        var actual = Convert.ToHexString(writer.Encode());
        Assert.AreEqual(expected, actual);
    }
}
