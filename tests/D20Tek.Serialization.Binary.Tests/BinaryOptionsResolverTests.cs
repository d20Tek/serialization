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

    // --- Built-in converter registration ---

    [TestMethod]
    public void Resolve_RegistersBuiltInConverters()
    {
        // arrange
        var options = new BinarySerializerOptions();

        // act
        var resolved = BinaryOptionsResolver.Resolve(options);

        // assert
        Assert.IsTrue(HasConverterFor<Guid>(resolved));
        Assert.IsTrue(HasConverterFor<DateTime>(resolved));
        Assert.IsTrue(HasConverterFor<DateTimeOffset>(resolved));
        Assert.IsTrue(HasConverterFor<decimal>(resolved));
    }

    [TestMethod]
    public void Resolve_DoesNotDuplicateBuiltIns_WhenCalledTwice()
    {
        // arrange
        var options = new BinarySerializerOptions();

        // act
        BinaryOptionsResolver.Resolve(options);
        BinaryOptionsResolver.Resolve(options);

        // assert — count should be 4 (one per built-in), not 8
        var guidCount = options.Converters.Count(c => c.CanConvert(typeof(Guid)));
        Assert.AreEqual(1, guidCount);
    }

    [TestMethod]
    public void Resolve_UserConverterTakesPrecedence()
    {
        // arrange
        var userConverter = new CustomGuidConverter();
        var options = new BinarySerializerOptions();
        options.Converters.Add(userConverter);

        // act
        BinaryOptionsResolver.Resolve(options);

        // assert — only the user's converter should be present for Guid
        var guidConverters = options.Converters.Where(c => c.CanConvert(typeof(Guid))).ToList();
        Assert.AreEqual(1, guidConverters.Count);
        Assert.AreSame(userConverter, guidConverters[0]);
    }

    [TestMethod]
    public void Resolve_UserDateTimeConverterTakesPrecedence()
    {
        // arrange
        var userConverter = new CustomDateTimeConverter();
        var options = new BinarySerializerOptions();
        options.Converters.Add(userConverter);

        // act
        BinaryOptionsResolver.Resolve(options);

        // assert — only the user's converter should be present for DateTime
        var converters = options.Converters.Where(c => c.CanConvert(typeof(DateTime))).ToList();
        Assert.AreEqual(1, converters.Count);
        Assert.AreSame(userConverter, converters[0]);
    }

    [TestMethod]
    public void Resolve_UserDateTimeOffsetConverterTakesPrecedence()
    {
        // arrange
        var userConverter = new CustomDateTimeOffsetConverter();
        var options = new BinarySerializerOptions();
        options.Converters.Add(userConverter);

        // act
        BinaryOptionsResolver.Resolve(options);

        // assert — only the user's converter should be present for DateTimeOffset
        var converters = options.Converters.Where(c => c.CanConvert(typeof(DateTimeOffset))).ToList();
        Assert.AreEqual(1, converters.Count);
        Assert.AreSame(userConverter, converters[0]);
    }

    [TestMethod]
    public void Resolve_UserDecimalConverterTakesPrecedence()
    {
        // arrange
        var userConverter = new CustomDecimalConverter();
        var options = new BinarySerializerOptions();
        options.Converters.Add(userConverter);

        // act
        BinaryOptionsResolver.Resolve(options);

        // assert — only the user's converter should be present for decimal
        var converters = options.Converters.Where(c => c.CanConvert(typeof(decimal))).ToList();
        Assert.AreEqual(1, converters.Count);
        Assert.AreSame(userConverter, converters[0]);
    }

    private static bool HasConverterFor<T>(BinarySerializerOptions options) =>
        options.Converters.Any(c => c.CanConvert(typeof(T)));

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

    [ExcludeFromCodeCoverage]
    private sealed class CustomGuidConverter : Converter<Guid>
    {
        public override Guid Read(IFormatReader reader, SerializerOptions options) =>
            throw new NotImplementedException();

        public override void Write(IFormatWriter writer, Guid value, SerializerOptions options) =>
            throw new NotImplementedException();
    }

    [ExcludeFromCodeCoverage]
    private sealed class CustomDateTimeConverter : Converter<DateTime>
    {
        public override DateTime Read(IFormatReader reader, SerializerOptions options) =>
            throw new NotImplementedException();

        public override void Write(IFormatWriter writer, DateTime value, SerializerOptions options) =>
            throw new NotImplementedException();
    }

    [ExcludeFromCodeCoverage]
    private sealed class CustomDateTimeOffsetConverter : Converter<DateTimeOffset>
    {
        public override DateTimeOffset Read(IFormatReader reader, SerializerOptions options) =>
            throw new NotImplementedException();

        public override void Write(IFormatWriter writer, DateTimeOffset value, SerializerOptions options) =>
            throw new NotImplementedException();
    }

    [ExcludeFromCodeCoverage]
    private sealed class CustomDecimalConverter : Converter<decimal>
    {
        public override decimal Read(IFormatReader reader, SerializerOptions options) =>
            throw new NotImplementedException();

        public override void Write(IFormatWriter writer, decimal value, SerializerOptions options) =>
            throw new NotImplementedException();
    }
}
