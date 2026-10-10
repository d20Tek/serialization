using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using static D20Tek.Serialization.Binary.Tests.BinarySerializerTestSupport;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
[SuppressMessage("Trimming", "IL2026")]
[SuppressMessage("AOT", "IL3050")]
public sealed class BinarySerializerRoundTripTests
{
    [TestMethod]
    public void SerializeToByteArray_Deserialize_RoundTrip()
    {
        // arrange
        var original = new SimpleModel { Name = "Alice", Age = 30, IsActive = true };

        // act
        var bytes = BinarySerializer.SerializeToByteArray(original);
        var result = BinarySerializer.Deserialize<SimpleModel>(bytes);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Alice", result.Name);
        Assert.AreEqual(30, result.Age);
        Assert.IsTrue(result.IsActive);
    }

    [TestMethod]
    public void SerializeToByteArray_Deserialize_NullOptions_RoundTrip()
    {
        // arrange
        var original = new SimpleModel { Name = "Bob", Age = 25, IsActive = false };

        // act
        var bytes = BinarySerializer.SerializeToByteArray(original, null);
        var result = BinarySerializer.Deserialize<SimpleModel>(bytes, null);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Bob", result.Name);
        Assert.AreEqual(25, result.Age);
        Assert.IsFalse(result.IsActive);
    }

    [TestMethod]
    public void SerializeToByteArray_Deserialize_WithOptions_RoundTrip()
    {
        // arrange
        var options = new BinarySerializerOptions
        {
            PropertyNamingPolicy = NamingPolicy.CamelCase,
        };
        var original = new SimpleModel { Name = "Charlie", Age = 40, IsActive = true };

        // act
        var bytes = BinarySerializer.SerializeToByteArray(original, options);
        var result = BinarySerializer.Deserialize<SimpleModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Charlie", result.Name);
        Assert.AreEqual(40, result.Age);
        Assert.IsTrue(result.IsActive);
    }

    [TestMethod]
    public void Serialize_IBufferWriter_RoundTrip()
    {
        // arrange
        var original = new SimpleModel { Name = "Diana", Age = 35, IsActive = true };
        var buffer = new ArrayBufferWriter<byte>();

        // act
        BinarySerializer.Serialize(buffer, original);
        var result = BinarySerializer.Deserialize<SimpleModel>(buffer.WrittenMemory);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Diana", result.Name);
        Assert.AreEqual(35, result.Age);
        Assert.IsTrue(result.IsActive);
    }

    [TestMethod]
    public void Serialize_IBufferWriter_NullBufferWriter_Throws()
    {
        // act & assert
        Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage] () =>
            BinarySerializer.Serialize((IBufferWriter<byte>)null!, new SimpleModel()));
    }

    [TestMethod]
    public void Deserialize_NonGeneric_RoundTrip()
    {
        // arrange
        var original = new SimpleModel { Name = "Eve", Age = 28, IsActive = false };
        var bytes = BinarySerializer.SerializeToByteArray(original);

        // act
        var result = BinarySerializer.Deserialize(bytes, typeof(SimpleModel)) as SimpleModel;

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Eve", result.Name);
        Assert.AreEqual(28, result.Age);
        Assert.IsFalse(result.IsActive);
    }

    [TestMethod]
    public void Deserialize_NonGeneric_NullType_Throws()
    {
        // act & assert
        Assert.ThrowsExactly<ArgumentNullException>(
            [ExcludeFromCodeCoverage] () =>
                BinarySerializer.Deserialize(Array.Empty<byte>(), null!));
    }

    [TestMethod]
    public void RoundTrip_NestedObject_PreservesValues()
    {
        // arrange
        var original = new ParentModel
        {
            Label = "Parent",
            Child = new SimpleModel { Name = "Child", Age = 5, IsActive = true },
        };

        // act
        var bytes = BinarySerializer.SerializeToByteArray(original);
        var result = BinarySerializer.Deserialize<ParentModel>(bytes);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Parent", result.Label);
        Assert.IsNotNull(result.Child);
        Assert.AreEqual("Child", result.Child.Name);
        Assert.AreEqual(5, result.Child.Age);
        Assert.IsTrue(result.Child.IsActive);
    }

    [TestMethod]
    public void RoundTrip_NullValue_ReturnsNull()
    {
        // act
        var bytes = BinarySerializer.SerializeToByteArray<SimpleModel?>(null);
        var result = BinarySerializer.Deserialize<SimpleModel?>(bytes);

        // assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void RoundTrip_Guid_UsesBuiltInConverter()
    {
        // arrange — serialize/deserialize a Guid directly so the facade's
        // converter resolution (not the reflection serializer) handles it.
        var original = Guid.Parse("01020304-0506-0708-090a-0b0c0d0e0f10");

        // act
        var bytes = BinarySerializer.SerializeToByteArray(original);
        var result = BinarySerializer.Deserialize<Guid>(bytes);

        // assert
        Assert.AreEqual(original, result);
    }
}
