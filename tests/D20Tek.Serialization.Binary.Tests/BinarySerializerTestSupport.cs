using D20Tek.Serialization.Generation;
using System.Diagnostics.CodeAnalysis;

[assembly: GeneratedSerializerRegistry(
    typeof(D20Tek.Serialization.Binary.Tests.BinarySerializerTestSupport.TestGeneratedRegistry))]

namespace D20Tek.Serialization.Binary.Tests;

/// <summary>
/// Shared test models and doubles used by the split <c>BinarySerializer*Tests</c> classes.
/// </summary>
internal static class BinarySerializerTestSupport
{
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

    public class GeneratedModel
    {
        public string? Value { get; set; }
        public int Count { get; set; }
    }

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
        public bool TryGetSerializer<T>([MaybeNullWhen(false)] out IGeneratedSerializer<T> serializer)
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

    /// <summary>
    /// A converter that tracks whether Read/Write were called, proving the custom converter
    /// tier takes precedence over generated and reflection tiers.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class TrackingSimpleModelConverter : Converter<SimpleModel>
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
