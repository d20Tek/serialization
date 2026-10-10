using D20Tek.Serialization.Binary.Reflection;
using System.Diagnostics.CodeAnalysis;
using static D20Tek.Serialization.Binary.Tests.ReflectionBinarySerializerTestSupport;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
[SuppressMessage("Trimming", "IL2026")]
[SuppressMessage("AOT", "IL3050")]
public sealed class ReflectionBinarySerializerRoundTripTests
{
    private readonly ReflectionBinarySerializer _serializer = new();

    [TestMethod]
    public void RoundTrip_SimpleObject_PreservesValues()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var original = new SimpleModel { Name = "Alice", Age = 30, IsActive = true };

        // act
        var bytes = _serializer.Serialize(original, options);
        var result = _serializer.Deserialize<SimpleModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Alice", result.Name);
        Assert.AreEqual(30, result.Age);
        Assert.IsTrue(result.IsActive);
    }

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
        var bytes = _serializer.Serialize(original, options);
        var result = _serializer.Deserialize<ParentModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("parent", result.Label);
        Assert.IsNotNull(result.Child);
        Assert.AreEqual("child", result.Child.Name);
        Assert.AreEqual(5, result.Child.Age);
        Assert.IsFalse(result.Child.IsActive);
    }

    [TestMethod]
    public void RoundTrip_NullProperty_PreservesNull()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var original = new ParentModel { Label = "test", Child = null };

        // act
        var bytes = _serializer.Serialize(original, options);
        var result = _serializer.Deserialize<ParentModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("test", result.Label);
        Assert.IsNull(result.Child);
    }

    [TestMethod]
    public void RoundTrip_EnumValue_PreservesValue()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var original = new EnumModel { Status = TestStatus.Active };

        // act
        var bytes = _serializer.Serialize(original, options);
        var result = _serializer.Deserialize<EnumModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual(TestStatus.Active, result.Status);
    }

    [TestMethod]
    public void RoundTrip_NullableValueType_WithValue()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var original = new NullableModel { Score = 99.5 };

        // act
        var bytes = _serializer.Serialize(original, options);
        var result = _serializer.Deserialize<NullableModel>(bytes, options);

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
        var bytes = _serializer.Serialize(original, options);
        var result = _serializer.Deserialize<NullableModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.IsNull(result.Score);
    }

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
        var bytes = _serializer.Serialize(original, options);
        var result = _serializer.Deserialize<IntegersModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual((byte)200, result.ByteVal);
        Assert.AreEqual((short)-1000, result.ShortVal);
        Assert.AreEqual(123456, result.IntVal);
        Assert.AreEqual(9876543210L, result.LongVal);
    }

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
        var bytes = _serializer.Serialize(original, options);
        var result = _serializer.Deserialize<FloatsModel>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1.5f, result.FloatVal);
        Assert.AreEqual(3.14159, result.DoubleVal, 0.00001);
    }
}
