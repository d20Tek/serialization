using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Core.Tests;

[TestClass]
public sealed class ConverterFactoryTests
{
    [TestMethod]
    public void CanConvert_ForFactory_ReturnsTrueForOpenGenericFamily()
    {
        // arrange
        var factory = new BoxConverterFactory();

        // act
        var result = factory.CanConvert(typeof(Box<int>));

        // assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    [DataRow(typeof(string))]
    [DataRow(typeof(int))]
    public void CanConvert_ForFactory_ReturnsFalseForUnrelatedTypes(Type typeToConvert)
    {
        // arrange
        var factory = new BoxConverterFactory();

        // act
        var result = factory.CanConvert(typeToConvert);

        // assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void CreateConverter_ForClosedGenericType_ReturnsConverterThatCanConvertThatType()
    {
        // arrange
        var factory = new BoxConverterFactory();

        // act
        var converter = factory.CreateConverter(typeof(Box<int>));

        // assert
        Assert.IsTrue(converter.CanConvert(typeof(Box<int>)));
    }

    [TestMethod]
    public void CreateConverter_ForDifferentClosedGenericTypes_ReturnsDistinctConverters()
    {
        // arrange
        var factory = new BoxConverterFactory();

        // act
        var intConverter = factory.CreateConverter(typeof(Box<int>));
        var stringConverter = factory.CreateConverter(typeof(Box<string>));

        // assert
        Assert.IsTrue(intConverter.CanConvert(typeof(Box<int>)));
        Assert.IsFalse(intConverter.CanConvert(typeof(Box<string>)));
        Assert.IsTrue(stringConverter.CanConvert(typeof(Box<string>)));
        Assert.IsFalse(stringConverter.CanConvert(typeof(Box<int>)));
    }

    [TestMethod]
    public void ConverterFactory_IsAConverter()
    {
        // arrange
        var factory = new BoxConverterFactory();

        // act & assert
        Assert.IsInstanceOfType<Converter>(factory);
    }

    // --- Test models ---

    public sealed class Box<T>
    {
        public T? Value { get; set; }
    }

    // --- Test doubles ---

    [ExcludeFromCodeCoverage]
    private sealed class BoxConverterFactory : ConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Box<>);

        public override Converter CreateConverter(Type typeToConvert)
        {
            var itemType = typeToConvert.GetGenericArguments()[0];
            var converterType = typeof(BoxConverter<>).MakeGenericType(itemType);
            return (Converter)Activator.CreateInstance(converterType)!;
        }
    }

    [ExcludeFromCodeCoverage]
    private sealed class BoxConverter<T> : Converter<Box<T>>
    {
        public override Box<T>? Read(IFormatReader reader, SerializerOptions options) =>
            throw new NotImplementedException();

        public override void Write(IFormatWriter writer, Box<T> value, SerializerOptions options) =>
            throw new NotImplementedException();
    }
}
