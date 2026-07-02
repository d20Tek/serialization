using D20Tek.Serialization.Binary.Cbor;
using D20Tek.Serialization.Binary.Cbor.Converters;

namespace D20Tek.Serialization.Binary.Tests.Cbor.Converters;

[TestClass]
public sealed class EnumCborConverterTests
{
    private readonly BinarySerializerOptions _options = new();

    [TestMethod]
    public void RoundTrip_PreservesValue()
    {
        // arrange
        var converter = new EnumCborConverter<DayOfWeek>();

        // act
        var bytes = WriteWith(converter, DayOfWeek.Wednesday);
        var result = ReadWith(converter, bytes);

        // assert
        Assert.AreEqual(DayOfWeek.Wednesday, result);
    }

    [TestMethod]
    public void RoundTrip_ZeroValue()
    {
        // arrange
        var converter = new EnumCborConverter<DayOfWeek>();

        // act
        var bytes = WriteWith(converter, DayOfWeek.Sunday);
        var result = ReadWith(converter, bytes);

        // assert
        Assert.AreEqual(DayOfWeek.Sunday, result);
    }

    [TestMethod]
    public void Write_EmitsInteger()
    {
        // arrange
        var converter = new EnumCborConverter<DayOfWeek>();

        // act
        var bytes = WriteWith(converter, DayOfWeek.Friday);

        // assert — Friday = 5, CBOR unsigned integer 5 is 0x05
        Assert.AreEqual(1, bytes.Length);
        Assert.AreEqual(0x05, bytes[0]);
    }

    [TestMethod]
    public void CanConvert()
    {
        var converter = new EnumCborConverter<DayOfWeek>();
        Assert.IsTrue(converter.CanConvert(typeof(DayOfWeek)));
        Assert.IsFalse(converter.CanConvert(typeof(int)));
        Assert.IsFalse(converter.CanConvert(typeof(ConsoleColor)));
    }

    private byte[] WriteWith<TEnum>(Converter<TEnum> converter, TEnum value) where TEnum : struct, Enum
    {
        var writer = new CborFormatWriter();
        converter.Write(writer, value, _options);
        return writer.Encode();
    }

    private TEnum ReadWith<TEnum>(Converter<TEnum> converter, byte[] bytes) where TEnum : struct, Enum
    {
        var reader = new CborFormatReader(bytes);
        return converter.Read(reader, _options)!;
    }
}
