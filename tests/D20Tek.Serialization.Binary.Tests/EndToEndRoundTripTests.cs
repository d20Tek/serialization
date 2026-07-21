using D20Tek.Serialization.Binary.Cbor.Converters;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
[SuppressMessage("Trimming", "IL2026")]
[SuppressMessage("AOT", "IL3050")]
public sealed class EndToEndRoundTripTests
{
    // --- Flat objects with primitive types ---

    [TestMethod]
    public void RoundTrip_FlatObject_AllPrimitiveTypes()
    {
        // arrange
        var original = new PrimitiveModel
        {
            Text = "hello",
            Integer = 42,
            LargeInteger = long.MaxValue,
            FloatingPoint = 3.14,
            Flag = true,
        };

        // act
        var bytes = BinarySerializer.SerializeToByteArray(original);
        var result = BinarySerializer.Deserialize<PrimitiveModel>(bytes);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("hello", result.Text);
        Assert.AreEqual(42, result.Integer);
        Assert.AreEqual(long.MaxValue, result.LargeInteger);
        Assert.AreEqual(3.14, result.FloatingPoint, 0.001);
        Assert.IsTrue(result.Flag);
    }

    // --- Nested objects ---

    [TestMethod]
    public void RoundTrip_DeeplyNestedObject_PreservesValues()
    {
        // arrange
        var original = new OuterModel
        {
            Name = "outer",
            Inner = new InnerModel
            {
                Value = 99,
                Child = new LeafModel { Tag = "leaf" },
            },
        };

        // act
        var bytes = BinarySerializer.SerializeToByteArray(original);
        var result = BinarySerializer.Deserialize<OuterModel>(bytes);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("outer", result.Name);
        Assert.IsNotNull(result.Inner);
        Assert.AreEqual(99, result.Inner.Value);
        Assert.IsNotNull(result.Inner.Child);
        Assert.AreEqual("leaf", result.Inner.Child.Tag);
    }

    // --- Null properties ---

    [TestMethod]
    public void RoundTrip_NullProperties_PreservedAsNull()
    {
        // arrange
        var original = new OuterModel { Name = "test", Inner = null };

        // act
        var bytes = BinarySerializer.SerializeToByteArray(original);
        var result = BinarySerializer.Deserialize<OuterModel>(bytes);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("test", result.Name);
        Assert.IsNull(result.Inner);
    }

    [TestMethod]
    public void RoundTrip_NullTopLevel_ReturnsNull()
    {
        // act
        var bytes = BinarySerializer.SerializeToByteArray<OuterModel?>(null);
        var result = BinarySerializer.Deserialize<OuterModel?>(bytes);

        // assert
        Assert.IsNull(result);
    }

    // --- Built-in converter: Guid ---

    [TestMethod]
    public void RoundTrip_Guid_ViaBuiltInConverter()
    {
        // arrange
        var original = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        // act
        var bytes = BinarySerializer.SerializeToByteArray(original);
        var result = BinarySerializer.Deserialize<Guid>(bytes);

        // assert
        Assert.AreEqual(original, result);
    }

    // --- Built-in converter: DateTime ---

    [TestMethod]
    public void RoundTrip_DateTime_ViaBuiltInConverter()
    {
        // arrange
        var original = new DateTime(2025, 6, 15, 12, 30, 0, DateTimeKind.Utc);

        // act
        var bytes = BinarySerializer.SerializeToByteArray(original);
        var result = BinarySerializer.Deserialize<DateTime>(bytes);

        // assert
        Assert.AreEqual(original, result);
    }

    // --- Built-in converter: DateTimeOffset ---

    [TestMethod]
    public void RoundTrip_DateTimeOffset_ViaBuiltInConverter()
    {
        // arrange
        var original = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // act
        var bytes = BinarySerializer.SerializeToByteArray(original);
        var result = BinarySerializer.Deserialize<DateTimeOffset>(bytes);

        // assert
        Assert.AreEqual(original, result);
    }

    // --- Built-in converter: decimal ---

    [TestMethod]
    public void RoundTrip_Decimal_ViaBuiltInConverter()
    {
        // arrange
        var original = 12345.6789m;

        // act
        var bytes = BinarySerializer.SerializeToByteArray(original);
        var result = BinarySerializer.Deserialize<decimal>(bytes);

        // assert
        Assert.AreEqual(original, result);
    }

    // --- Enum via explicit converter registration ---

    [TestMethod]
    public void RoundTrip_Enum_ViaExplicitConverter()
    {
        // arrange
        var options = new BinarySerializerOptions();
        options.Converters.Add(new EnumCborConverter<TestColor>());
        var original = TestColor.Green;

        // act
        var bytes = BinarySerializer.SerializeToByteArray(original, options);
        var result = BinarySerializer.Deserialize<TestColor>(bytes, options);

        // assert
        Assert.AreEqual(original, result);
    }

    // --- Object containing multiple primitive types ---

    [TestMethod]
    public void RoundTrip_ObjectWithMultiplePrimitiveTypes_PreservesAll()
    {
        // arrange
        var original = new MultiTypeModel
        {
            Label = "composite",
            Count = 42,
            LargeCount = 9_876_543_210L,
            Ratio = 0.75,
            Active = true,
            Description = null,
        };

        // act
        var bytes = BinarySerializer.SerializeToByteArray(original);
        var result = BinarySerializer.Deserialize<MultiTypeModel>(bytes);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("composite", result.Label);
        Assert.AreEqual(42, result.Count);
        Assert.AreEqual(9_876_543_210L, result.LargeCount);
        Assert.AreEqual(0.75, result.Ratio, 0.001);
        Assert.IsTrue(result.Active);
        Assert.IsNull(result.Description);
    }

    // --- CamelCase naming policy ---

    [TestMethod]
    public void RoundTrip_CamelCaseNamingPolicy_PreservesValues()
    {
        // arrange
        var options = new BinarySerializerOptions
        {
            PropertyNamingPolicy = NamingPolicy.CamelCase,
        };
        var original = new PrimitiveModel
        {
            Text = "camel",
            Integer = 7,
            LargeInteger = 100L,
            FloatingPoint = 2.5,
            Flag = false,
        };

        // act
        var bytes = BinarySerializer.SerializeToByteArray(original, options);
        var result = BinarySerializer.Deserialize<PrimitiveModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("camel", result.Text);
        Assert.AreEqual(7, result.Integer);
        Assert.AreEqual(100L, result.LargeInteger);
        Assert.AreEqual(2.5, result.FloatingPoint, 0.001);
        Assert.IsFalse(result.Flag);
    }

    // --- IgnoreNullValues option ---

    [TestMethod]
    public void RoundTrip_IgnoreNullValues_OmitsAndPreservesDefaults()
    {
        // arrange
        var options = new BinarySerializerOptions { IgnoreNullValues = true };
        var original = new OuterModel { Name = "test", Inner = null };

        // act
        var bytes = BinarySerializer.SerializeToByteArray(original, options);
        var result = BinarySerializer.Deserialize<OuterModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("test", result.Name);
        Assert.IsNull(result.Inner);
    }

    // --- Multiple round-trips produce identical output ---

    [TestMethod]
    public void RoundTrip_DeterministicOutput_SameBytesForSameInput()
    {
        // arrange
        var model = new PrimitiveModel
        {
            Text = "deterministic",
            Integer = 1,
            LargeInteger = 2L,
            FloatingPoint = 3.0,
            Flag = true,
        };

        // act
        var bytes1 = BinarySerializer.SerializeToByteArray(model);
        var bytes2 = BinarySerializer.SerializeToByteArray(model);

        // assert
        CollectionAssert.AreEqual(bytes1, bytes2);
    }

    // --- Test models ---

    public class PrimitiveModel
    {
        public string? Text { get; set; }
        public int Integer { get; set; }
        public long LargeInteger { get; set; }
        public double FloatingPoint { get; set; }
        public bool Flag { get; set; }
    }

    public class OuterModel
    {
        public string? Name { get; set; }
        public InnerModel? Inner { get; set; }
    }

    public class InnerModel
    {
        public int Value { get; set; }
        public LeafModel? Child { get; set; }
    }

    public class LeafModel
    {
        public string? Tag { get; set; }
    }

    public enum TestColor { Red, Green, Blue }

    public class MultiTypeModel
    {
        public string? Label { get; set; }
        public int Count { get; set; }
        public long LargeCount { get; set; }
        public double Ratio { get; set; }
        public bool Active { get; set; }
        public string? Description { get; set; }
    }
}
