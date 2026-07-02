using D20Tek.Serialization.Binary.Cbor;
using D20Tek.Serialization.Binary.Cbor.Converters;

namespace D20Tek.Serialization.Binary.Tests.Cbor.Converters;

[TestClass]
public sealed class DecimalCborConverterTests
{
    private readonly BinarySerializerOptions _options = new();

    [TestMethod]
    public void RoundTrip_PreservesValue()
    {
        // arrange
        var converter = new DecimalCborConverter();
        var original = 12345.6789m;

        // act
        var bytes = WriteWith(converter, original);
        var result = ReadWith(converter, bytes);

        // assert
        Assert.AreEqual(original, result);
    }

    [TestMethod]
    public void RoundTrip_NegativeValue()
    {
        // arrange
        var converter = new DecimalCborConverter();
        var original = -99999.00001m;

        // act
        var bytes = WriteWith(converter, original);
        var result = ReadWith(converter, bytes);

        // assert
        Assert.AreEqual(original, result);
    }

    [TestMethod]
    public void Write_EmitsTextString()
    {
        // arrange
        var converter = new DecimalCborConverter();

        // act
        var bytes = WriteWith(converter, 1.5m);

        // assert — CBOR text string starts with major type 3 (0x60–0x77 for short strings)
        Assert.AreEqual(0x60, bytes[0] & 0xE0, "Expected CBOR text string major type.");
    }

    [TestMethod]
    public void CanConvert()
    {
        var converter = new DecimalCborConverter();
        Assert.IsTrue(converter.CanConvert(typeof(decimal)));
        Assert.IsFalse(converter.CanConvert(typeof(double)));
    }

    private byte[] WriteWith(Converter<decimal> converter, decimal value)
    {
        var writer = new CborFormatWriter();
        converter.Write(writer, value, _options);
        return writer.Encode();
    }

    private decimal ReadWith(Converter<decimal> converter, byte[] bytes)
    {
        var reader = new CborFormatReader(bytes);
        return converter.Read(reader, _options)!;
    }
}
