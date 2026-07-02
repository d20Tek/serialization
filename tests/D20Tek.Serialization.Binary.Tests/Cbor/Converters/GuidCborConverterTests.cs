using D20Tek.Serialization.Binary.Cbor;
using D20Tek.Serialization.Binary.Cbor.Converters;
using System.Diagnostics.CodeAnalysis;
using System.Formats.Cbor;

namespace D20Tek.Serialization.Binary.Tests.Cbor.Converters;

[TestClass]
public sealed class GuidCborConverterTests
{
    private readonly BinarySerializerOptions _options = new();

    [TestMethod]
    public void RoundTrip_PreservesValue()
    {
        // arrange
        var converter = new GuidCborConverter();
        var original = Guid.Parse("01020304-0506-0708-090a-0b0c0d0e0f10");

        // act
        var bytes = WriteWith(converter, original);
        var result = ReadWith(converter, bytes);

        // assert
        Assert.AreEqual(original, result);
    }

    [TestMethod]
    public void Write_EmitsTag37AndByteString()
    {
        // arrange
        var converter = new GuidCborConverter();
        var guid = Guid.Parse("01020304-0506-0708-090a-0b0c0d0e0f10");

        // act
        var bytes = WriteWith(converter, guid);

        // assert — first byte is Tag 37 (0xD8 0x25), followed by a 16-byte bstr (0x50 + 16 bytes)
        Assert.AreEqual(0xD8, bytes[0]); // Tag major type 6, 1-byte extended
        Assert.AreEqual(0x25, bytes[1]); // Tag value 37
        Assert.AreEqual(0x50, bytes[2]); // Byte string major type 2, length 16
        Assert.AreEqual(19, bytes.Length); // 2 (tag) + 1 (bstr header) + 16 (payload)
    }

    [TestMethod]
    public void Read_WrongTag_Throws()
    {
        // arrange — write Tag 99 + 16-byte bstr
        var cbor = new CborWriter(CborConformanceMode.Lax);
        cbor.WriteTag((CborTag)99);
        cbor.WriteByteString(new byte[16]);
        var bytes = cbor.Encode();
        var converter = new GuidCborConverter();

        // act & assert
        var ex = Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() =>
            ReadWith(converter, bytes));
        Assert.IsTrue(ex.Message.Contains("37"));
    }

    [TestMethod]
    public void CanConvert_ReturnsTrueForGuid()
    {
        var converter = new GuidCborConverter();
        Assert.IsTrue(converter.CanConvert(typeof(Guid)));
        Assert.IsFalse(converter.CanConvert(typeof(string)));
    }

    private byte[] WriteWith(Converter<Guid> converter, Guid value)
    {
        var writer = new CborFormatWriter();
        converter.Write(writer, value, _options);
        return writer.Encode();
    }

    private Guid ReadWith(Converter<Guid> converter, byte[] bytes)
    {
        var reader = new CborFormatReader(bytes);
        return converter.Read(reader, _options)!;
    }
}
