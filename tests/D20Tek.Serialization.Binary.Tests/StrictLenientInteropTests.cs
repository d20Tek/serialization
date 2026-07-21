using D20Tek.Serialization.Binary.Cbor;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
[SuppressMessage("Trimming", "IL2026")]
[SuppressMessage("AOT", "IL3050")]
public sealed class StrictLenientInteropTests
{
    // --- Forward compatibility: extra fields in payload ---

    [TestMethod]
    public void Lenient_ExtraFields_SkippedSuccessfully()
    {
        // arrange — serialize a model with more properties
        var bytes = BinarySerializer.SerializeToByteArray(
            new FullModel { Name = "Alice", Age = 30, Extra = "ignored" });

        // act — deserialize into a model that lacks the extra property (lenient mode)
        var options = new BinarySerializerOptions { DecodingMode = BinaryDecodingMode.Lenient };
        var result = BinarySerializer.Deserialize<SlimModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Alice", result.Name);
        Assert.AreEqual(30, result.Age);
    }

    [TestMethod]
    public void Strict_ExtraFields_DeserializesWhenReaderSkipsUnknownProperties()
    {
        // arrange — serialize with extra properties
        var bytes = BinarySerializer.SerializeToByteArray(
            new FullModel { Name = "Bob", Age = 25, Extra = "oops" });

        // act — strict mode: the reflection serializer encounters unknown property "Extra"
        // and calls SkipValue, which in strict mode throws on tags but not on plain values.
        // Unknown properties themselves are skipped by the reflection serializer's switch,
        // but the SkipValue call on a tagged value would throw. Here Extra is a plain string,
        // so it should still succeed.
        var options = new BinarySerializerOptions { DecodingMode = BinaryDecodingMode.Strict };
        var result = BinarySerializer.Deserialize<SlimModel>(bytes, options);

        // assert — strict mode still deserializes when extra fields are plain (non-tagged)
        Assert.IsNotNull(result);
        Assert.AreEqual("Bob", result.Name);
        Assert.AreEqual(25, result.Age);
    }

    // --- Strict mode rejects unknown CBOR tags ---

    [TestMethod]
    public void Strict_UnknownTag_ThrowsSerializationException()
    {
        // arrange — manually build CBOR with an unknown tag (e.g., Tag 999) wrapping a value
        var writer = new CborFormatWriter();
        writer.WriteStartObject();
        writer.WritePropertyName("Name");
        writer.WriteString("Test");
        writer.WritePropertyName("Tagged");
        writer.WriteTag((System.Formats.Cbor.CborTag)999);
        writer.WriteNumber(42L);
        writer.WriteEndObject();
        var data = writer.Encode();

        // act — strict mode reader: when SkipValue encounters the tag, it should throw
        var reader = new CborFormatReader(data, BinaryDecodingMode.Strict);
        reader.ReadStartObject();

        // Read first property
        Assert.IsTrue(reader.TryReadPropertyName(out var name1));
        Assert.AreEqual("Name", name1);
        _ = reader.GetString();

        // Read second property with unknown tag
        Assert.IsTrue(reader.TryReadPropertyName(out var name2));
        Assert.AreEqual("Tagged", name2);

        // assert — SkipValue in strict mode throws on the unknown tag
        Assert.ThrowsExactly<SerializationException>([ExcludeFromCodeCoverage]() => reader.SkipValue());
    }

    [TestMethod]
    public void Lenient_UnknownTag_SkippedSuccessfully()
    {
        // arrange — same CBOR with unknown tag
        var writer = new CborFormatWriter();
        writer.WriteStartObject();
        writer.WritePropertyName("Name");
        writer.WriteString("Test");
        writer.WritePropertyName("Tagged");
        writer.WriteTag((System.Formats.Cbor.CborTag)999);
        writer.WriteNumber(42L);
        writer.WriteEndObject();
        var data = writer.Encode();

        // act — lenient mode reader: SkipValue should silently skip the tagged value
        var reader = new CborFormatReader(data, BinaryDecodingMode.Lenient);
        reader.ReadStartObject();

        Assert.IsTrue(reader.TryReadPropertyName(out _));
        _ = reader.GetString();

        Assert.IsTrue(reader.TryReadPropertyName(out _));
        reader.SkipValue(); // should not throw

        // assert — reader is at end of object
        reader.ReadEndObject();
    }

    // --- Backward compatibility: missing fields use defaults ---

    [TestMethod]
    public void Lenient_MissingFields_DefaultValues()
    {
        // arrange — serialize a slim model
        var bytes = BinarySerializer.SerializeToByteArray(
            new SlimModel { Name = "Charlie", Age = 40 });

        // act — deserialize into a model with more properties
        var options = new BinarySerializerOptions { DecodingMode = BinaryDecodingMode.Lenient };
        var result = BinarySerializer.Deserialize<FullModel>(bytes, options);

        // assert — missing "Extra" gets its default value (null)
        Assert.IsNotNull(result);
        Assert.AreEqual("Charlie", result.Name);
        Assert.AreEqual(40, result.Age);
        Assert.IsNull(result.Extra);
    }

    // --- Strict mode with nested objects and extra fields ---

    [TestMethod]
    public void Lenient_NestedExtraFields_SkippedSuccessfully()
    {
        // arrange — serialize a parent with a full child
        var bytes = BinarySerializer.SerializeToByteArray(
            new ParentFull
            {
                Label = "parent",
                Child = new FullModel { Name = "child", Age = 5, Extra = "skipped" },
            });

        // act — deserialize with slim child model in lenient mode
        var options = new BinarySerializerOptions { DecodingMode = BinaryDecodingMode.Lenient };
        var result = BinarySerializer.Deserialize<ParentSlim>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("parent", result.Label);
        Assert.IsNotNull(result.Child);
        Assert.AreEqual("child", result.Child.Name);
        Assert.AreEqual(5, result.Child.Age);
    }

    // --- Lenient mode with multiple unknown properties ---

    [TestMethod]
    public void Lenient_MultipleUnknownProperties_AllSkipped()
    {
        // arrange — build CBOR with several unknown properties
        var writer = new CborFormatWriter();
        writer.WriteStartObject();
        writer.WritePropertyName("Name");
        writer.WriteString("Known");
        writer.WritePropertyName("Unknown1");
        writer.WriteNumber(100L);
        writer.WritePropertyName("Age");
        writer.WriteNumber(20L);
        writer.WritePropertyName("Unknown2");
        writer.WriteBoolean(true);
        writer.WritePropertyName("Unknown3");
        writer.WriteNull();
        writer.WriteEndObject();
        var data = writer.Encode();

        // act
        var options = new BinarySerializerOptions { DecodingMode = BinaryDecodingMode.Lenient };
        var result = BinarySerializer.Deserialize<SlimModel>(data, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Known", result.Name);
        Assert.AreEqual(20, result.Age);
    }

    // --- Test models ---

    public class SlimModel
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    public class FullModel
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public string? Extra { get; set; }
    }

    public class ParentFull
    {
        public string? Label { get; set; }
        public FullModel? Child { get; set; }
    }

    public class ParentSlim
    {
        public string? Label { get; set; }
        public SlimModel? Child { get; set; }
    }
}
