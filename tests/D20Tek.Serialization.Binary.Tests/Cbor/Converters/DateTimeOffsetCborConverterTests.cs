using D20Tek.Serialization.Binary.Cbor;
using D20Tek.Serialization.Binary.Cbor.Converters;
using System.Diagnostics.CodeAnalysis;
using System.Formats.Cbor;

namespace D20Tek.Serialization.Binary.Tests.Cbor.Converters;

[TestClass]
public sealed class DateTimeOffsetCborConverterTests
{
    private readonly BinarySerializerOptions _options = new();

    [TestMethod]
    public void RoundTrip_PreservesUtcValue()
    {
        // arrange
        var converter = new DateTimeOffsetCborConverter();
        var original = new DateTimeOffset(2025, 7, 1, 12, 30, 45, TimeSpan.Zero);

        // act
        var bytes = WriteWith(converter, original);
        var result = ReadWith(converter, bytes);

        // assert
        Assert.AreEqual(original.Year, result.Year);
        Assert.AreEqual(original.Month, result.Month);
        Assert.AreEqual(original.Day, result.Day);
        Assert.AreEqual(original.Hour, result.Hour);
        Assert.AreEqual(original.Minute, result.Minute);
        Assert.AreEqual(original.Second, result.Second);
        Assert.AreEqual(TimeSpan.Zero, result.Offset);
    }

    [TestMethod]
    public void RoundTrip_WithOffset_NormalizesToUtc()
    {
        // arrange
        var converter = new DateTimeOffsetCborConverter();
        var original = new DateTimeOffset(2025, 7, 1, 14, 0, 0, TimeSpan.FromHours(2));
        // UTC is 12:00

        // act
        var bytes = WriteWith(converter, original);
        var result = ReadWith(converter, bytes);

        // assert — round-trip yields UTC
        Assert.AreEqual(12, result.Hour);
        Assert.AreEqual(TimeSpan.Zero, result.Offset);
    }

    [TestMethod]
    public void Write_EmitsTag1()
    {
        // arrange
        var converter = new DateTimeOffsetCborConverter();
        var dto = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // act
        var bytes = WriteWith(converter, dto);

        // assert
        Assert.AreEqual(0xC1, bytes[0]);
    }

    [TestMethod]
    public void Read_WrongTag_Throws()
    {
        // arrange
        var cbor = new CborWriter(CborConformanceMode.Lax);
        cbor.WriteTag((CborTag)99);
        cbor.WriteInt64(0);
        var bytes = cbor.Encode();
        var converter = new DateTimeOffsetCborConverter();

        // act & assert
        var ex = Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() =>
            ReadWith(converter, bytes));
        Assert.Contains("tag", ex.Message);
    }

    [TestMethod]
    public void CanConvert()
    {
        var converter = new DateTimeOffsetCborConverter();
        Assert.IsTrue(converter.CanConvert(typeof(DateTimeOffset)));
        Assert.IsFalse(converter.CanConvert(typeof(DateTime)));
    }

    private byte[] WriteWith(Converter<DateTimeOffset> converter, DateTimeOffset value)
    {
        var writer = new CborFormatWriter();
        converter.Write(writer, value, _options);
        return writer.Encode();
    }

    private DateTimeOffset ReadWith(Converter<DateTimeOffset> converter, byte[] bytes)
    {
        var reader = new CborFormatReader(bytes);
        return converter.Read(reader, _options)!;
    }
}
