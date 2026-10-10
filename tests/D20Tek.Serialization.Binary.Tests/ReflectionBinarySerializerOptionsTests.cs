using D20Tek.Serialization.Binary.Reflection;
using System.Diagnostics.CodeAnalysis;
using static D20Tek.Serialization.Binary.Tests.ReflectionBinarySerializerTestSupport;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
[SuppressMessage("Trimming", "IL2026")]
[SuppressMessage("AOT", "IL3050")]
public sealed class ReflectionBinarySerializerOptionsTests
{
    private readonly ReflectionBinarySerializer _serializer = new();

    [TestMethod]
    public void Write_IgnoreNullValues_OmitsNullProperties()
    {
        // arrange
        var options = new BinarySerializerOptions { IgnoreNullValues = true };
        var original = new ParentModel { Label = "test", Child = null };

        // act - serialize with ignore nulls then deserialize
        var bytes = _serializer.Serialize(original, options);
        var result = _serializer.Deserialize<ParentModel>(bytes, options);

        // assert - should round-trip; omitted null leaves default
        Assert.IsNotNull(result);
        Assert.AreEqual("test", result.Label);
        Assert.IsNull(result.Child);
    }

    [TestMethod]
    public void RoundTrip_CamelCaseNamingPolicy_UsesTransformedNames()
    {
        // arrange
        var options = new BinarySerializerOptions { PropertyNamingPolicy = NamingPolicy.CamelCase };
        var original = new SimpleModel { Name = "Bob", Age = 25, IsActive = true };

        // act
        var bytes = _serializer.Serialize(original, options);
        var result = _serializer.Deserialize<SimpleModel>(bytes, options);

        // assert - round-trip works with consistent naming policy
        Assert.IsNotNull(result);
        Assert.AreEqual("Bob", result.Name);
        Assert.AreEqual(25, result.Age);
        Assert.IsTrue(result.IsActive);
    }

    [TestMethod]
    public void RoundTrip_IncludeFields_SerializesPublicFields()
    {
        // arrange
        var options = new BinarySerializerOptions { IncludeFields = true };
        var original = new ModelWithFields { Id = 42, Description = "hello" };

        // act
        var bytes = _serializer.Serialize(original, options);
        var result = _serializer.Deserialize<ModelWithFields>(bytes, options);

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
        var bytes = _serializer.Serialize(original, options);
        var result = _serializer.Deserialize<ModelWithFields>(bytes, options);

        // assert - only properties are serialized; fields remain default
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Id);            // field not serialized
        Assert.IsNull(result.Description);         // field not serialized
    }

    [TestMethod]
    public void RoundTrip_NameSerializedAttribute_UsesCustomName()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var original = new CustomNameModel { Value = "hello" };

        // act
        var bytes = _serializer.Serialize(original, options);
        var result = _serializer.Deserialize<CustomNameModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("hello", result.Value);
    }

    [TestMethod]
    public void RoundTrip_IgnoreSerializedAttribute_OmitsMember()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var original = new IgnoredMemberModel { Visible = "shown", Secret = "hidden" };

        // act
        var bytes = _serializer.Serialize(original, options);
        var result = _serializer.Deserialize<IgnoredMemberModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("shown", result.Visible);
        Assert.IsNull(result.Secret); // was ignored during serialization
    }
}
