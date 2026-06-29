using D20Tek.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
public sealed class BinarySerializerOptionsTests
{
    [TestMethod]
    public void Defaults_AreApplied()
    {
        // arrange

        // act
        var options = new BinarySerializerOptions();

        // assert
        Assert.IsFalse(options.IncludeFields);
        Assert.AreEqual(BinaryDecodingMode.Lenient, options.DecodingMode);
        Assert.IsNull(options.Profile);
    }

    [TestMethod]
    public void Defaults_InheritedFromSerializerOptions_AreApplied()
    {
        // arrange

        // act
        var options = new BinarySerializerOptions();

        // assert
        Assert.IsNull(options.PropertyNamingPolicy);
        Assert.IsFalse(options.IgnoreNullValues);
        Assert.IsNotNull(options.Converters);
        Assert.IsEmpty(options.Converters);
    }

    [TestMethod]
    public void IsSerializerOptions()
    {
        // arrange

        // act
        var options = new BinarySerializerOptions();

        // assert
        Assert.IsInstanceOfType<SerializerOptions>(options);
    }

    [TestMethod]
    public void IncludeFields_CanBeAssigned()
    {
        // arrange

        // act
        var options = new BinarySerializerOptions { IncludeFields = true };

        // assert
        Assert.IsTrue(options.IncludeFields);
    }

    [TestMethod]
    public void DecodingMode_CanBeAssigned()
    {
        // arrange

        // act
        var options = new BinarySerializerOptions { DecodingMode = BinaryDecodingMode.Strict };

        // assert
        Assert.AreEqual(BinaryDecodingMode.Strict, options.DecodingMode);
    }

    [TestMethod]
    public void Profile_CanBeAssigned()
    {
        // arrange
        var profile = new TestBinaryProfile();

        // act
        var options = new BinarySerializerOptions { Profile = profile };

        // assert
        Assert.AreSame(profile, options.Profile);
    }

    [ExcludeFromCodeCoverage]
    private sealed class TestBinaryProfile : BinaryProfile
    {
        public override string Name => nameof(TestBinaryProfile);
    }
}
