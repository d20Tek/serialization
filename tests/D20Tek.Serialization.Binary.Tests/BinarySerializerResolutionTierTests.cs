using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using static D20Tek.Serialization.Binary.Tests.BinarySerializerTestSupport;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
[SuppressMessage("Trimming", "IL2026")]
[SuppressMessage("AOT", "IL3050")]
public sealed class BinarySerializerResolutionTierTests
{
    // --- Resolution order: custom converter takes precedence ---

    [TestMethod]
    public void Serialize_CustomConverterTakesPrecedence()
    {
        // arrange
        var converter = new TrackingSimpleModelConverter();
        var options = new BinarySerializerOptions();
        options.Converters.Add(converter);
        var value = new SimpleModel { Name = "Test", Age = 1, IsActive = true };

        // act
        _ = BinarySerializer.SerializeToByteArray(value, options);

        // assert — the custom converter was used, not reflection
        Assert.IsTrue(converter.WriteCalled);
    }

    [TestMethod]
    public void Deserialize_CustomConverterTakesPrecedence()
    {
        // arrange — serialize via reflection, then deserialize with custom converter
        var bytes = BinarySerializer.SerializeToByteArray(
            new SimpleModel { Name = "Test", Age = 1, IsActive = true });

        var converter = new TrackingSimpleModelConverter();
        var options = new BinarySerializerOptions();
        options.Converters.Add(converter);

        // act
        _ = BinarySerializer.Deserialize<SimpleModel>(bytes, options);

        // assert — the custom converter was used
        Assert.IsTrue(converter.ReadCalled);
    }

    [TestMethod]
    public void Deserialize_NonGeneric_CustomConverterTakesPrecedence()
    {
        // arrange — serialize via reflection, then deserialize with custom converter
        var bytes = BinarySerializer.SerializeToByteArray(
            new SimpleModel { Name = "Test", Age = 1, IsActive = true });

        var converter = new TrackingSimpleModelConverter();
        var options = new BinarySerializerOptions();
        options.Converters.Add(converter);

        // act
        _ = BinarySerializer.Deserialize(bytes, typeof(SimpleModel), options);

        // assert — the custom converter was used
        Assert.IsTrue(converter.ReadCalled);
    }

    // --- Resolution order: reflection fallback used when no converter or generated ---

    [TestMethod]
    public void Serialize_ReflectionFallback_WhenNoConverterOrGenerated()
    {
        // arrange — no custom converter, no generated serializer for SimpleModel
        var original = new SimpleModel { Name = "Reflect", Age = 99, IsActive = true };

        // act — should succeed via reflection
        var bytes = BinarySerializer.SerializeToByteArray(original);
        var result = BinarySerializer.Deserialize<SimpleModel>(bytes);

        // assert — round-trip proves reflection worked
        Assert.IsNotNull(result);
        Assert.AreEqual("Reflect", result.Name);
        Assert.AreEqual(99, result.Age);
        Assert.IsTrue(result.IsActive);
    }

    // --- Generated serializer tier ---

    [TestMethod]
    public void Deserialize_UsesGeneratedSerializer_WhenRegistryHasMatch()
    {
        // arrange — serialize via the generated serializer, then deserialize through the facade
        var original = new GeneratedModel { Value = "Generated", Count = 42 };
        var bytes = BinarySerializer.SerializeToByteArray(original);

        // act
        var result = BinarySerializer.Deserialize<GeneratedModel>(bytes);

        // assert — the generated serializer tier handled both write and read
        Assert.IsNotNull(result);
        Assert.AreEqual("Generated", result.Value);
        Assert.AreEqual(42, result.Count);
    }

    [TestMethod]
    public void Serialize_UsesGeneratedSerializer_WhenRegistryHasMatch()
    {
        // arrange
        var original = new GeneratedModel { Value = "Gen", Count = 7 };
        var buffer = new ArrayBufferWriter<byte>();

        // act — serialize via IBufferWriter, then round-trip back
        BinarySerializer.Serialize(buffer, original);
        var result = BinarySerializer.Deserialize<GeneratedModel>(buffer.WrittenMemory);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Gen", result.Value);
        Assert.AreEqual(7, result.Count);
    }
}
