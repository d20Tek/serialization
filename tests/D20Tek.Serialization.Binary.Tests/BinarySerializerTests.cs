using D20Tek.Serialization.Generation;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

[assembly: GeneratedSerializerRegistry(typeof(D20Tek.Serialization.Binary.Tests.BinarySerializerTests.TestGeneratedRegistry))]

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
[SuppressMessage("Trimming", "IL2026")]
[SuppressMessage("AOT", "IL3050")]
public sealed class BinarySerializerTests
{
    // --- SerializeToByteArray / Deserialize<T> round-trip ---

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

    // --- Serialize to IBufferWriter ---

    [TestMethod]
    public void Serialize_IBufferWriter_RoundTrip()
    {
        // arrange
        var original = new SimpleModel { Name = "Diana", Age = 35, IsActive = true };
        var buffer = new ArrayBufferWriter<byte>();

        // act
        BinarySerializer.Serialize(buffer, original);
        var result = BinarySerializer.Deserialize<SimpleModel>(buffer.WrittenSpan);

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
            BinarySerializer.Serialize(null!, new SimpleModel()));
    }

    // --- Non-generic Deserialize ---

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
                BinarySerializer.Deserialize([], null!));
    }

    // --- Nested object round-trip ---

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

    // --- Null value round-trip ---

    [TestMethod]
    public void RoundTrip_NullValue_ReturnsNull()
    {
        // act
        var bytes = BinarySerializer.SerializeToByteArray<SimpleModel?>(null);
        var result = BinarySerializer.Deserialize<SimpleModel?>(bytes);

        // assert
        Assert.IsNull(result);
    }

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

    // --- Built-in converters integrate through facade ---

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
        var result = BinarySerializer.Deserialize<GeneratedModel>(buffer.WrittenSpan);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Gen", result.Value);
        Assert.AreEqual(7, result.Count);
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

    public class GuidModel
    {
        public Guid Id { get; set; }
    }

    // --- Test doubles ---

    /// <summary>
    /// A converter that tracks whether Read/Write were called, proving the custom converter
    /// tier takes precedence over generated and reflection tiers.
    /// </summary>
    public class GeneratedModel
    {
        public string? Value { get; set; }
        public int Count { get; set; }
    }

    // --- Test doubles ---

    /// <summary>
    /// A fake generated serializer for <see cref="GeneratedModel"/> that proves the
    /// generated-serializer resolution tier is exercised by the facade.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class TestGeneratedSerializer : IGeneratedSerializer<GeneratedModel>
    {
        public void Write(IFormatWriter writer, GeneratedModel value, SerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Value");
            writer.WriteString(value.Value ?? string.Empty);
            writer.WritePropertyName("Count");
            writer.WriteNumber((long)value.Count);
            writer.WriteEndObject();
        }

        public GeneratedModel Read(IFormatReader reader, SerializerOptions options)
        {
            var model = new GeneratedModel();
            reader.ReadStartObject();
            while (reader.TryReadPropertyName(out var name))
            {
                switch (name)
                {
                    case "Value": model.Value = reader.GetString(); break;
                    case "Count": model.Count = (int)reader.GetInt64(); break;
                    default: reader.SkipValue(); break;
                }
            }

            reader.ReadEndObject();
            return model;
        }
    }

    /// <summary>
    /// A fake registry that returns <see cref="TestGeneratedSerializer"/> for
    /// <see cref="GeneratedModel"/> and nothing for any other type.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class TestGeneratedRegistry : IGeneratedSerializerRegistry
    {
        public bool TryGetSerializer<T>([System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out IGeneratedSerializer<T> serializer)
        {
            if (typeof(T) == typeof(GeneratedModel))
            {
                serializer = (IGeneratedSerializer<T>)(object)new TestGeneratedSerializer();
                return true;
            }

            serializer = null;
            return false;
        }
    }

    [ExcludeFromCodeCoverage]
    private sealed class TrackingSimpleModelConverter : Converter<SimpleModel>
    {
        public bool WriteCalled { get; private set; }
        public bool ReadCalled { get; private set; }

        public override void Write(IFormatWriter writer, SimpleModel value, SerializerOptions options)
        {
            WriteCalled = true;

            // Write a minimal CBOR object so deserialization doesn't fail
            writer.WriteStartObject();
            writer.WritePropertyName("Name");
            writer.WriteString(value.Name ?? string.Empty);
            writer.WritePropertyName("Age");
            writer.WriteNumber((long)value.Age);
            writer.WritePropertyName("IsActive");
            writer.WriteBoolean(value.IsActive);
            writer.WriteEndObject();
        }

        public override SimpleModel Read(IFormatReader reader, SerializerOptions options)
        {
            ReadCalled = true;

            var model = new SimpleModel();
            reader.ReadStartObject();
            while (reader.TryReadPropertyName(out var name))
            {
                switch (name)
                {
                    case "Name": model.Name = reader.GetString(); break;
                    case "Age": model.Age = (int)reader.GetInt64(); break;
                    case "IsActive": model.IsActive = reader.GetBoolean(); break;
                    default: reader.SkipValue(); break;
                }
            }

            reader.ReadEndObject();
            return model;
        }
    }
}
