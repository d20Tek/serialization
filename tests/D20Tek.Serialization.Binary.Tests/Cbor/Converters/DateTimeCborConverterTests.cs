using D20Tek.Serialization.Binary.Cbor;
using D20Tek.Serialization.Binary.Cbor.Converters;
using System.Diagnostics.CodeAnalysis;
using System.Formats.Cbor;

namespace D20Tek.Serialization.Binary.Tests.Cbor.Converters;

[TestClass]
public sealed class DateTimeCborConverterTests
{
    private readonly BinarySerializerOptions _options = new();

    [TestMethod]
    public void RoundTrip_PreservesUtcValue()
    {
        // arrange
        var converter = new DateTimeCborConverter();
        var original = new DateTime(2025, 7, 1, 12, 30, 45, DateTimeKind.Utc);

        // act
        var bytes = WriteWith(converter, original);
        var result = ReadWith(converter, bytes);

        // assert — millisecond precision
        Assert.AreEqual(original.Year, result.Year);
        Assert.AreEqual(original.Month, result.Month);
        Assert.AreEqual(original.Day, result.Day);
        Assert.AreEqual(original.Hour, result.Hour);
        Assert.AreEqual(original.Minute, result.Minute);
        Assert.AreEqual(original.Second, result.Second);
    }

    [TestMethod]
    public void Write_EmitsTag1()
    {
        // arrange
        var converter = new DateTimeCborConverter();
        var dt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // act
        var bytes = WriteWith(converter, dt);

        // assert — Tag 1 is a single byte 0xC1
        Assert.AreEqual(0xC1, bytes[0]);
    }

    [TestMethod]
    public void RoundTrip_LocalTimeConvertedToUtc()
    {
        // arrange
        var converter = new DateTimeCborConverter();
        var local = new DateTime(2025, 7, 1, 12, 0, 0, DateTimeKind.Local);
        var expectedUtc = local.ToUniversalTime();

        // act
        var bytes = WriteWith(converter, local);
        var result = ReadWith(converter, bytes);

        // assert — result should match UTC equivalent
        Assert.AreEqual(expectedUtc.Year, result.Year);
        Assert.AreEqual(expectedUtc.Month, result.Month);
        Assert.AreEqual(expectedUtc.Day, result.Day);
        Assert.AreEqual(expectedUtc.Hour, result.Hour);
    }

    [TestMethod]
    public void Read_WrongTag_Throws()
    {
        // arrange — write Tag 99 + integer
        var cbor = new CborWriter(CborConformanceMode.Lax);
        cbor.WriteTag((CborTag)99);
        cbor.WriteInt64(0);
        var bytes = cbor.Encode();
        var converter = new DateTimeCborConverter();

        // act & assert
        var ex = Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() =>
            ReadWith(converter, bytes));
        Assert.Contains("tag", ex.Message);
    }

    [TestMethod]
    public void CanConvert()
    {
        var converter = new DateTimeCborConverter();
        Assert.IsTrue(converter.CanConvert(typeof(DateTime)));
        Assert.IsFalse(converter.CanConvert(typeof(DateTimeOffset)));
    }

    private byte[] WriteWith(Converter<DateTime> converter, DateTime value)
    {
        var writer = new CborFormatWriter();
        converter.Write(writer, value, _options);
        return writer.Encode();
    }

    private DateTime ReadWith(Converter<DateTime> converter, byte[] bytes)
    {
        var reader = new CborFormatReader(bytes);
        return converter.Read(reader, _options)!;
    }
}
