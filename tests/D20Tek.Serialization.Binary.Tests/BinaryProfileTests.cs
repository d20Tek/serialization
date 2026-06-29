using D20Tek.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
public sealed class BinaryProfileTests
{
    [TestMethod]
    public void Name_ReturnsConcreteValue()
    {
        // arrange
        var profile = new NamedProfile();

        // act
        var name = profile.Name;

        // assert
        Assert.AreEqual("named-profile", name);
    }

    [TestMethod]
    public void Configure_DefaultImplementation_DoesNotModifyOptions()
    {
        // arrange
        var profile = new NoOpProfile();
        var options = new BinarySerializerOptions();

        // act
        profile.Configure(options);

        // assert
        Assert.IsFalse(options.IncludeFields);
        Assert.AreEqual(BinaryDecodingMode.Lenient, options.DecodingMode);
        Assert.IsNull(options.PropertyNamingPolicy);
        Assert.IsFalse(options.IgnoreNullValues);
    }

    [TestMethod]
    public void Configure_Override_MutatesOptions()
    {
        // arrange
        var profile = new StrictFieldsProfile();
        var options = new BinarySerializerOptions();

        // act
        profile.Configure(options);

        // assert
        Assert.IsTrue(options.IncludeFields);
        Assert.AreEqual(BinaryDecodingMode.Strict, options.DecodingMode);
    }

    private sealed class NamedProfile : BinaryProfile
    {
        public override string Name => "named-profile";
    }

    [ExcludeFromCodeCoverage]
    private sealed class NoOpProfile : BinaryProfile
    {
        public override string Name => nameof(NoOpProfile);
    }

    [ExcludeFromCodeCoverage]
    private sealed class StrictFieldsProfile : BinaryProfile
    {
        public override string Name => nameof(StrictFieldsProfile);

        public override void Configure(BinarySerializerOptions options)
        {
            options.IncludeFields = true;
            options.DecodingMode = BinaryDecodingMode.Strict;
        }
    }
}
