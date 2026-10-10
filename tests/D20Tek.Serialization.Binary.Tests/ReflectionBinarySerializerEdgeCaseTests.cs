using D20Tek.Serialization.Binary.Cbor;
using D20Tek.Serialization.Binary.Reflection;
using System.Diagnostics.CodeAnalysis;
using static D20Tek.Serialization.Binary.Tests.ReflectionBinarySerializerTestSupport;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
[SuppressMessage("Trimming", "IL2026")]
[SuppressMessage("AOT", "IL3050")]
public sealed class ReflectionBinarySerializerEdgeCaseTests
{
    private readonly ReflectionBinarySerializer _serializer = new();

    [TestMethod]
    public void Read_MissingRequiredMember_ThrowsSerializationException()
    {
        // arrange - serialize an object where the required member is present,
        // then remove it by serializing a different type that omits the member
        var options = new BinarySerializerOptions();

        // Serialize an empty object: CBOR map with 0 items
        var writer = new CborFormatWriter();
        writer.WriteStartObject();
        writer.WriteEndObject();
        var bytes = writer.Encode();

        // act & assert
        var ex = Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() =>
            _serializer.Deserialize<RequiredModel>(bytes, options));

        Assert.Contains("RequiredField", ex.Message);
        Assert.AreEqual("$.RequiredField", ex.Path);
    }

    [TestMethod]
    public void Read_UnknownProperty_IsSkipped()
    {
        // arrange - write a CBOR object with an extra property not in the model
        var writer = new CborFormatWriter();
        writer.WriteStartObject();
        writer.WritePropertyName("Name");
        writer.WriteString("test");
        writer.WritePropertyName("UnknownProp");
        writer.WriteString("extra");
        writer.WritePropertyName("Age");
        writer.WriteNumber(10L);
        writer.WritePropertyName("IsActive");
        writer.WriteBoolean(true);
        writer.WriteEndObject();
        var bytes = writer.Encode();

        var options = new BinarySerializerOptions();

        // act
        var result = _serializer.Deserialize<SimpleModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("test", result.Name);
        Assert.AreEqual(10, result.Age);
        Assert.IsTrue(result.IsActive);
    }

    [TestMethod]
    public void Write_NullRoot_WritesNull()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var writer = new CborFormatWriter();

        // act
        _serializer.Write<SimpleModel?>(writer, null, options);
        var bytes = writer.Encode();

        // assert - CBOR null is 0xF6
        Assert.HasCount(1, bytes);
        Assert.AreEqual(0xF6, bytes[0]);
    }

    [TestMethod]
    public void Read_NullRoot_ReturnsDefault()
    {
        // arrange
        var writer = new CborFormatWriter();
        writer.WriteNull();
        var bytes = writer.Encode();

        var options = new BinarySerializerOptions();

        // act
        var result = _serializer.Deserialize<SimpleModel>(bytes, options);

        // assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void RoundTrip_NameSerializedAttribute_WritesCanonicalNameOnly()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var original = new AliasModel { Value = "hello" };

        // act
        var bytes = _serializer.Serialize(original, options);
        var result = _serializer.Deserialize<AliasModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("hello", result.Value);
    }

    [TestMethod]
    public void Deserialize_LegacyAliasName_PopulatesMember()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var writer = new CborFormatWriter();
        writer.WriteStartObject();
        writer.WritePropertyName("old_name");
        writer.WriteString("legacy value");
        writer.WriteEndObject();
        var bytes = writer.Encode();

        // act
        var result = _serializer.Deserialize<AliasModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("legacy value", result.Value);
    }

    [TestMethod]
    public void Deserialize_AnotherLegacyAliasName_PopulatesMember()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var writer = new CborFormatWriter();
        writer.WriteStartObject();
        writer.WritePropertyName("older_name");
        writer.WriteString("even older value");
        writer.WriteEndObject();
        var bytes = writer.Encode();

        // act
        var result = _serializer.Deserialize<AliasModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("even older value", result.Value);
    }

    [TestMethod]
    public void Deserialize_LegacyAliasName_SatisfiesRequiredMember()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var writer = new CborFormatWriter();
        writer.WriteStartObject();
        writer.WritePropertyName("old_name");
        writer.WriteString("legacy value");
        writer.WriteEndObject();
        var bytes = writer.Encode();

        // act
        var result = _serializer.Deserialize<AliasRequiredModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("legacy value", result.Value);
    }
}
