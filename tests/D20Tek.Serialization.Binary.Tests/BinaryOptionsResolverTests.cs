using D20Tek.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
public sealed class BinaryOptionsResolverTests
{
    [TestMethod]
    public void Resolve_NullOptions_ReturnsDefaults()
    {
        // arrange

        // act
        var resolved = BinaryOptionsResolver.Resolve(null);

        // assert
        Assert.IsNotNull(resolved);
        Assert.IsFalse(resolved.IncludeFields);
        Assert.AreEqual(BinaryDecodingMode.Lenient, resolved.DecodingMode);
        Assert.IsNull(resolved.Profile);
    }

    [TestMethod]
    public void Resolve_ExistingOptions_ReturnsSameInstance()
    {
        // arrange
        var options = new BinarySerializerOptions();

        // act
        var resolved = BinaryOptionsResolver.Resolve(options);

        // assert
        Assert.AreSame(options, resolved);
    }

    [TestMethod]
    public void Resolve_NoProfile_DoesNotMutateOptions()
    {
        // arrange
        var options = new BinarySerializerOptions
        {
            IncludeFields = true,
            DecodingMode = BinaryDecodingMode.Strict,
        };

        // act
        var resolved = BinaryOptionsResolver.Resolve(options);

        // assert
        Assert.IsTrue(resolved.IncludeFields);
        Assert.AreEqual(BinaryDecodingMode.Strict, resolved.DecodingMode);
    }

    [TestMethod]
    public void Resolve_WithProfile_AppliesProfileConfiguration()
    {
        // arrange
        var options = new BinarySerializerOptions { Profile = new StrictFieldsProfile() };

        // act
        var resolved = BinaryOptionsResolver.Resolve(options);

        // assert
        Assert.IsTrue(resolved.IncludeFields);
        Assert.AreEqual(BinaryDecodingMode.Strict, resolved.DecodingMode);
    }

    [TestMethod]
    public void Resolve_WithProfile_ConfiguresTheResolvedInstance()
    {
        // arrange
        var profile = new RecordingProfile();
        var options = new BinarySerializerOptions { Profile = profile };

        // act
        var resolved = BinaryOptionsResolver.Resolve(options);

        // assert
        Assert.AreSame(resolved, profile.ConfiguredOptions);
    }

    [TestMethod]
    public void Resolve_NullOptionsWithoutProfile_DoesNotThrow()
    {
        // arrange

        // act
        var resolved = BinaryOptionsResolver.Resolve(null);

        // assert
        Assert.IsNull(resolved.Profile);
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

    [ExcludeFromCodeCoverage]
    private sealed class RecordingProfile : BinaryProfile
    {
        public override string Name => nameof(RecordingProfile);

        public BinarySerializerOptions? ConfiguredOptions { get; private set; }

        public override void Configure(BinarySerializerOptions options) => ConfiguredOptions = options;
    }
}
