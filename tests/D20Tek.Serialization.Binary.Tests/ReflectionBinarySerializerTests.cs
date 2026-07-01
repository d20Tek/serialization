using D20Tek.Serialization.Binary.Cbor;
using D20Tek.Serialization.Binary.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
[SuppressMessage("Trimming", "IL2026")]
[SuppressMessage("AOT", "IL3050")]
public sealed class ReflectionBinarySerializerTests
{
    private readonly ReflectionBinarySerializer _serializer = new();

    // --- Round-trip: simple flat object ---

    [TestMethod]
    public void RoundTrip_SimpleObject_PreservesValues()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var original = new SimpleModel { Name = "Alice", Age = 30, IsActive = true };

        // act
        var bytes = Serialize(original, options);
        var result = Deserialize<SimpleModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Alice", result.Name);
        Assert.AreEqual(30, result.Age);
        Assert.IsTrue(result.IsActive);
    }

    // --- Round-trip: nested object ---

    [TestMethod]
    public void RoundTrip_NestedObject_PreservesValues()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var original = new ParentModel
        {
            Label = "parent",
            Child = new SimpleModel { Name = "child", Age = 5, IsActive = false }
        };

        // act
        var bytes = Serialize(original, options);
        var result = Deserialize<ParentModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("parent", result.Label);
        Assert.IsNotNull(result.Child);
        Assert.AreEqual("child", result.Child.Name);
        Assert.AreEqual(5, result.Child.Age);
        Assert.IsFalse(result.Child.IsActive);
    }

    // --- Null value handling ---

    [TestMethod]
    public void RoundTrip_NullProperty_PreservesNull()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var original = new ParentModel { Label = "test", Child = null };

        // act
        var bytes = Serialize(original, options);
        var result = Deserialize<ParentModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("test", result.Label);
        Assert.IsNull(result.Child);
    }

    // --- IgnoreNullValues option ---

    [TestMethod]
    public void Write_IgnoreNullValues_OmitsNullProperties()
    {
        // arrange
        var options = new BinarySerializerOptions { IgnoreNullValues = true };
        var original = new ParentModel { Label = "test", Child = null };

        // act - serialize with ignore nulls then deserialize
        var bytes = Serialize(original, options);
        var result = Deserialize<ParentModel>(bytes, options);

        // assert - should round-trip; omitted null leaves default
        Assert.IsNotNull(result);
        Assert.AreEqual("test", result.Label);
        Assert.IsNull(result.Child);
    }

    // --- Naming policy (camelCase) ---

    [TestMethod]
    public void RoundTrip_CamelCaseNamingPolicy_UsesTransformedNames()
    {
        // arrange
        var options = new BinarySerializerOptions { PropertyNamingPolicy = NamingPolicy.CamelCase };
        var original = new SimpleModel { Name = "Bob", Age = 25, IsActive = true };

        // act
        var bytes = Serialize(original, options);
        var result = Deserialize<SimpleModel>(bytes, options);

        // assert - round-trip works with consistent naming policy
        Assert.IsNotNull(result);
        Assert.AreEqual("Bob", result.Name);
        Assert.AreEqual(25, result.Age);
        Assert.IsTrue(result.IsActive);
    }

    // --- IncludeFields ---

    [TestMethod]
    public void RoundTrip_IncludeFields_SerializesPublicFields()
    {
        // arrange
        var options = new BinarySerializerOptions { IncludeFields = true };
        var original = new ModelWithFields { Id = 42, Description = "hello" };

        // act
        var bytes = Serialize(original, options);
        var result = Deserialize<ModelWithFields>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual(42, result.Id);
        Assert.AreEqual("hello", result.Description);
    }

    [TestMethod]
    public void RoundTrip_WithoutIncludeFields_IgnoresPublicFields()
    {
        // arrange
        var options = new BinarySerializerOptions { IncludeFields = false };
        var original = new ModelWithFields { Id = 99, Description = "should be ignored" };

        // act
        var bytes = Serialize(original, options);
        var result = Deserialize<ModelWithFields>(bytes, options);

        // assert - only properties are serialized; fields remain default
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Id);            // field not serialized
        Assert.IsNull(result.Description);         // field not serialized
    }

    // --- Required member enforcement ---

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
            Deserialize<RequiredModel>(bytes, options));

        Assert.Contains("RequiredField", ex.Message);
        Assert.AreEqual("$.RequiredField", ex.Path);
    }

    // --- Unknown property skipping ---

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
        var result = Deserialize<SimpleModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("test", result.Name);
        Assert.AreEqual(10, result.Age);
        Assert.IsTrue(result.IsActive);
    }

    // --- Enum support ---

    [TestMethod]
    public void RoundTrip_EnumValue_PreservesValue()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var original = new EnumModel { Status = TestStatus.Active };

        // act
        var bytes = Serialize(original, options);
        var result = Deserialize<EnumModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual(TestStatus.Active, result.Status);
    }

    // --- Nullable value type ---

    [TestMethod]
    public void RoundTrip_NullableValueType_WithValue()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var original = new NullableModel { Score = 99.5 };

        // act
        var bytes = Serialize(original, options);
        var result = Deserialize<NullableModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual(99.5, result.Score);
    }

    [TestMethod]
    public void RoundTrip_NullableValueType_Null()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var original = new NullableModel { Score = null };

        // act
        var bytes = Serialize(original, options);
        var result = Deserialize<NullableModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.IsNull(result.Score);
    }

    // --- Write null root ---

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

    // --- Read null root ---

    [TestMethod]
    public void Read_NullRoot_ReturnsDefault()
    {
        // arrange
        var writer = new CborFormatWriter();
        writer.WriteNull();
        var bytes = writer.Encode();

        var options = new BinarySerializerOptions();

        // act
        var result = Deserialize<SimpleModel>(bytes, options);

        // assert
        Assert.IsNull(result);
    }

    // --- Various integer types ---

    [TestMethod]
    public void RoundTrip_VariousIntegerTypes()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var original = new IntegersModel
        {
            ByteVal = 200,
            ShortVal = -1000,
            IntVal = 123456,
            LongVal = 9876543210L
        };

        // act
        var bytes = Serialize(original, options);
        var result = Deserialize<IntegersModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual((byte)200, result.ByteVal);
        Assert.AreEqual((short)-1000, result.ShortVal);
        Assert.AreEqual(123456, result.IntVal);
        Assert.AreEqual(9876543210L, result.LongVal);
    }

    // --- Various floating-point types ---

    [TestMethod]
    public void RoundTrip_VariousFloatTypes()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var original = new FloatsModel
        {
            FloatVal = 1.5f,
            DoubleVal = 3.14159
        };

        // act
        var bytes = Serialize(original, options);
        var result = Deserialize<FloatsModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1.5f, result.FloatVal);
        Assert.AreEqual(3.14159, result.DoubleVal, 0.00001);
    }

    // --- NameSerialized attribute ---

    [TestMethod]
    public void RoundTrip_NameSerializedAttribute_UsesCustomName()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var original = new CustomNameModel { Value = "hello" };

        // act
        var bytes = Serialize(original, options);
        var result = Deserialize<CustomNameModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("hello", result.Value);
    }

    // --- IgnoreSerialized attribute ---

    [TestMethod]
    public void RoundTrip_IgnoreSerializedAttribute_OmitsMember()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var original = new IgnoredMemberModel { Visible = "shown", Secret = "hidden" };

        // act
        var bytes = Serialize(original, options);
        var result = Deserialize<IgnoredMemberModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("shown", result.Visible);
        Assert.IsNull(result.Secret); // was ignored during serialization
    }

    // --- Helpers ---

    private byte[] Serialize<T>(T value, BinarySerializerOptions options)
    {
        var writer = new CborFormatWriter();
        _serializer.Write(writer, value, options);
        return writer.Encode();
    }

    private T? Deserialize<T>(byte[] bytes, BinarySerializerOptions options)
    {
        var reader = new CborFormatReader(bytes);
        return _serializer.Read<T>(reader, options);
    }

    // --- Test models ---

    public class SimpleModel
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    public class ParentModel
    {
        public string? Label { get; set; }
        public SimpleModel? Child { get; set; }
    }

    public class ModelWithFields
    {
        public int Id;
        public string? Description;

        // A property to ensure it's always serialized
        public string? Tag { get; set; }
    }

    public class RequiredModel
    {
        [RequiredSerialized]
        public string? RequiredField { get; set; }
    }

    public enum TestStatus
    {
        Inactive = 0,
        Active = 1,
        Suspended = 2
    }

    public class EnumModel
    {
        public TestStatus Status { get; set; }
    }

    public class NullableModel
    {
        public double? Score { get; set; }
    }

    public class IntegersModel
    {
        public byte ByteVal { get; set; }
        public short ShortVal { get; set; }
        public int IntVal { get; set; }
        public long LongVal { get; set; }
    }

    public class FloatsModel
    {
        public float FloatVal { get; set; }
        public double DoubleVal { get; set; }
    }

    public class CustomNameModel
    {
        [NameSerialized("custom_value")]
        public string? Value { get; set; }
    }

    public class IgnoredMemberModel
    {
        public string? Visible { get; set; }

        [IgnoreSerialized]
        public string? Secret { get; set; }
    }
}
