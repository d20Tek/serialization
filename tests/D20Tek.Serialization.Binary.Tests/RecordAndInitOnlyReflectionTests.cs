using D20Tek.Serialization.Binary.Cbor;
using D20Tek.Serialization.Binary.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Binary.Tests;

/// <summary>
/// Audits the reflection-based serialization fallback's behavior for <c>record</c> types and
/// <c>init</c>-only property accessors, since <see cref="TypeMetadataBuilder"/> was primarily
/// designed around mutable POCOs with <c>get; set;</c> accessors.
/// </summary>
[TestClass]
[SuppressMessage("Trimming", "IL2026")]
[SuppressMessage("AOT", "IL3050")]
public sealed class RecordAndInitOnlyReflectionTests
{
    private readonly ReflectionBinarySerializer _serializer = new();

    [TestMethod]
    public void RoundTrip_RecordWithMutableProperties_PreservesValues()
    {
        // arrange — a record with ordinary get; set; properties behaves like a class
        var options = new BinarySerializerOptions();
        var original = new MutableRecord { Id = 1, Name = "Alice" };

        // act
        var bytes = Serialize(original, options);
        var result = Deserialize<MutableRecord>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
        Assert.AreEqual("Alice", result.Name);
    }

    [TestMethod]
    public void RoundTrip_RecordWithInitOnlyProperties_PreservesValues()
    {
        // arrange — init-only accessors are a compile-time (C#) restriction only; the compiled
        // `set` accessor is invoked the same way at the IL/reflection level, so
        // Expression.Assign-based setters compiled by TypeMetadataBuilder can still write to
        // them via Activator.CreateInstance() + property set.
        var options = new BinarySerializerOptions();
        var original = new InitOnlyRecord { Id = 2, Name = "Bob" };

        // act
        var bytes = Serialize(original, options);
        var result = Deserialize<InitOnlyRecord>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Id);
        Assert.AreEqual("Bob", result.Name);
    }

    [TestMethod]
    public void RoundTrip_PositionalRecord_WithoutParameterlessConstructor_ThrowsOnDeserialize()
    {
        // arrange — a positional record's primary constructor requires arguments, and
        // ReadObject calls Activator.CreateInstance(actualType) with no args, so there is no
        // parameterless constructor to satisfy.
        var options = new BinarySerializerOptions();
        var original = new PositionalRecord(3, "Carol");

        // act
        var bytes = Serialize(original, options);

        // assert — documents the current gap: positional records without a parameterless
        // constructor are not supported by the reflection fallback.
        Assert.ThrowsExactly<MissingMethodException>(
            [ExcludeFromCodeCoverage] () => Deserialize<PositionalRecord>(bytes, options));
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

    public sealed record MutableRecord
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public sealed record InitOnlyRecord
    {
        public int Id { get; init; }
        public string? Name { get; init; }
    }

    public sealed record PositionalRecord(int Id, string? Name);
}
