using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Core.Tests;

[TestClass]
public sealed class ConverterTests
{
    [TestMethod]
    public void CanConvert_ForGenericConverter_ReturnsTrueForMatchingType()
    {
        // arrange
        var converter = new SampleConverter();

        // act
        var result = converter.CanConvert(typeof(Sample));

        // assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    [DataRow(typeof(string))]
    [DataRow(typeof(int))]
    [DataRow(typeof(object))]
    public void CanConvert_ForGenericConverter_ReturnsFalseForOtherTypes(Type typeToConvert)
    {
        // arrange
        var converter = new SampleConverter();

        // act
        var result = converter.CanConvert(typeToConvert);

        // assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void CanConvert_ForGenericConverter_ReturnsFalseForDerivedType()
    {
        // arrange
        var converter = new SampleConverter();

        // act
        var result = converter.CanConvert(typeof(DerivedSample));

        // assert
        Assert.IsFalse(result);
    }

    private class Sample
    {
    }

    private sealed class DerivedSample : Sample
    {
    }

    [ExcludeFromCodeCoverage]
    private sealed class SampleConverter : Converter<Sample>
    {
        public override Sample? Read(IFormatReader reader, SerializerOptions options) =>
            throw new NotImplementedException();

        public override void Write(IFormatWriter writer, Sample value, SerializerOptions options) =>
            throw new NotImplementedException();
    }
}
