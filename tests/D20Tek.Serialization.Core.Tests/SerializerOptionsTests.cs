using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Core.Tests;

[TestClass]
public sealed class SerializerOptionsTests
{
    [TestMethod]
    public void Defaults_AreApplied()
    {
        // arrange

        // act
        var options = new TestSerializerOptions();

        // assert
        Assert.IsNull(options.PropertyNamingPolicy);
        Assert.IsFalse(options.IgnoreNullValues);
        Assert.IsNotNull(options.Converters);
        Assert.IsEmpty(options.Converters);
    }

    [TestMethod]
    public void PropertyNamingPolicy_CanBeAssigned()
    {
        // arrange

        // act
        var options = new TestSerializerOptions
        {
            PropertyNamingPolicy = NamingPolicy.CamelCase,
        };

        // assert
        Assert.AreSame(NamingPolicy.CamelCase, options.PropertyNamingPolicy);
    }

    [TestMethod]
    public void IgnoreNullValues_CanBeAssigned()
    {
        // arrange

        // act
        var options = new TestSerializerOptions { IgnoreNullValues = true };

        // assert
        Assert.IsTrue(options.IgnoreNullValues);
    }

    [TestMethod]
    public void Converters_AreMutableAndPreserveAddedItems()
    {
        // arrange
        var options = new TestSerializerOptions();
        var converter = new TestConverter();

        // act
        options.Converters.Add(converter);

        // assert
        Assert.HasCount(1, options.Converters);
        Assert.AreSame(converter, options.Converters[0]);
    }

    private sealed class TestSerializerOptions : SerializerOptions
    {
    }

    [ExcludeFromCodeCoverage]
    private sealed class TestConverter : Converter
    {
        public override bool CanConvert(Type typeToConvert) => false;
    }
}
