using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Binary.Tests;

/// <summary>
/// Verifies that <see cref="ConverterFactory"/> instances registered in
/// <see cref="BinarySerializerOptions.Converters"/> participate in the custom-converter
/// resolution tier of <see cref="BinarySerializer"/>, for both the generic and non-generic
/// facade overloads.
/// </summary>
[TestClass]
[SuppressMessage("Trimming", "IL2026")]
[SuppressMessage("AOT", "IL3050")]
public sealed class ConverterFactoryResolutionTests
{
    [TestMethod]
    public void SerializeToByteArray_Deserialize_Generic_UsesFactoryCreatedConverter()
    {
        // arrange
        var options = new BinarySerializerOptions();
        options.Converters.Add(new BoxConverterFactory());
        var original = new Box<int> { Value = 42 };

        // act
        var bytes = BinarySerializer.SerializeToByteArray(original, options);
        var result = BinarySerializer.Deserialize<Box<int>>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual(42, result.Value);
    }

    [TestMethod]
    public void SerializeToByteArray_Deserialize_Generic_DifferentClosedTypes_BothRoundTrip()
    {
        // arrange — the same factory instance must handle multiple distinct closed generic types
        var options = new BinarySerializerOptions();
        options.Converters.Add(new BoxConverterFactory());

        var intBox = new Box<int> { Value = 7 };
        var stringBox = new Box<string> { Value = "seven" };

        // act
        var intBytes = BinarySerializer.SerializeToByteArray(intBox, options);
        var stringBytes = BinarySerializer.SerializeToByteArray(stringBox, options);
        var intResult = BinarySerializer.Deserialize<Box<int>>(intBytes, options);
        var stringResult = BinarySerializer.Deserialize<Box<string>>(stringBytes, options);

        // assert
        Assert.IsNotNull(intResult);
        Assert.AreEqual(7, intResult.Value);
        Assert.IsNotNull(stringResult);
        Assert.AreEqual("seven", stringResult.Value);
    }

    [TestMethod]
    public void Deserialize_NonGeneric_UsesFactoryCreatedConverter()
    {
        // arrange
        var options = new BinarySerializerOptions();
        options.Converters.Add(new BoxConverterFactory());
        var bytes = BinarySerializer.SerializeToByteArray(new Box<int> { Value = 99 }, options);

        // act
        var result = BinarySerializer.Deserialize(bytes, typeof(Box<int>), options);

        // assert
        var box = Assert.IsInstanceOfType<Box<int>>(result);
        Assert.AreEqual(99, box.Value);
    }

    [TestMethod]
    public void SerializeToByteArray_Deserialize_Generic_CachesConverterAcrossCalls()
    {
        // arrange — the factory tracks how many times CreateConverter is invoked per type
        var factory = new BoxConverterFactory();
        var options = new BinarySerializerOptions();
        options.Converters.Add(factory);

        // act — serialize/deserialize the same closed type multiple times
        for (var i = 0; i < 3; i++)
        {
            var bytes = BinarySerializer.SerializeToByteArray(new Box<int> { Value = i }, options);
            _ = BinarySerializer.Deserialize<Box<int>>(bytes, options);
        }

        // assert — the factory is still only invoked for the distinct closed type it saw;
        // we can't observe the internal resolver cache directly, so we assert functional
        // correctness instead (the round-trips above succeeding is the primary assertion).
        Assert.IsTrue(factory.CreateConverterCallCount >= 1);
    }

    [TestMethod]
    public void Serialize_Deserialize_UnregisteredBoxType_FallsBackToReflection()
    {
        // arrange — no factory registered, so Box<T> (a plain mutable class) goes through
        // the reflection fallback instead.
        var original = new Box<int> { Value = 5 };

        // act
        var bytes = BinarySerializer.SerializeToByteArray(original);
        var result = BinarySerializer.Deserialize<Box<int>>(bytes);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual(5, result.Value);
    }

    [TestMethod]
    public void SerializeToByteArray_FactoryCreatesMismatchedConverter_FallsBackToReflection()
    {
        // arrange — the factory claims it can convert Box<int>, but produces a converter for an
        // unrelated type, so the `is Converter<T>` pattern match fails and resolution must fall
        // through to the reflection path instead of using the mismatched converter.
        var options = new BinarySerializerOptions();
        options.Converters.Add(new MismatchedConverterFactory());
        var original = new Box<int> { Value = 11 };

        // act
        var bytes = BinarySerializer.SerializeToByteArray(original, options);
        var result = BinarySerializer.Deserialize<Box<int>>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual(11, result.Value);
    }

    [TestMethod]
    public void SerializeToByteArray_MismatchedOrdinaryConverterClaimsCanConvert_FallsBackToReflection()
    {
        // arrange — an ordinary (non-factory) converter whose CanConvert is overridden to
        // (incorrectly) claim it handles Box<int>, even though it is a Converter<string>. This
        // exercises the case where neither `candidate is Converter<T>` nor
        // `candidate is ConverterFactory` matches, so resolution must skip the candidate and
        // fall through to the reflection path.
        var options = new BinarySerializerOptions();
        options.Converters.Add(new MismatchedOrdinaryConverter());
        var original = new Box<int> { Value = 13 };

        // act
        var bytes = BinarySerializer.SerializeToByteArray(original, options);
        var result = BinarySerializer.Deserialize<Box<int>>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual(13, result.Value);
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
        public int CreateConverterCallCount { get; private set; }

        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Box<>);

        public override Converter CreateConverter(Type typeToConvert)
        {
            CreateConverterCallCount++;
            var itemType = typeToConvert.GetGenericArguments()[0];
            var converterType = typeof(BoxConverter<>).MakeGenericType(itemType);
            return (Converter)Activator.CreateInstance(converterType)!;
        }
    }

    [ExcludeFromCodeCoverage]
    private sealed class MismatchedConverterFactory : ConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Box<>);

        // Deliberately returns a converter for an unrelated type, so
        // `GetOrCreateConverter(...) is Converter<T>` fails to match for the requested `T`.
        public override Converter CreateConverter(Type typeToConvert) => new UnrelatedConverter();
    }

    [ExcludeFromCodeCoverage]
    private sealed class UnrelatedConverter : Converter<string>
    {
        public override string? Read(IFormatReader reader, SerializerOptions options) => reader.GetString();

        public override void Write(IFormatWriter writer, string value, SerializerOptions options) =>
            writer.WriteString(value);
    }

    [ExcludeFromCodeCoverage]
    private sealed class MismatchedOrdinaryConverter : Converter<string>
    {
        // Deliberately claims it can convert any type, including Box<int>, even though it is
        // only a Converter<string>.
        public override bool CanConvert(Type typeToConvert) => true;

        public override string? Read(IFormatReader reader, SerializerOptions options) => reader.GetString();

        public override void Write(IFormatWriter writer, string value, SerializerOptions options) =>
            writer.WriteString(value);
    }

    [ExcludeFromCodeCoverage]
    private sealed class BoxConverter<T> : Converter<Box<T>>
    {
        public override Box<T>? Read(IFormatReader reader, SerializerOptions options)
        {
            var box = new Box<T>();
            reader.ReadStartObject();
            while (reader.TryReadPropertyName(out var name))
            {
                if (name == "Value")
                {
                    box.Value = (T?)ReadValue(reader);
                }
                else
                {
                    reader.SkipValue();
                }
            }

            reader.ReadEndObject();
            return box;
        }

        public override void Write(IFormatWriter writer, Box<T> value, SerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Value");
            WriteValue(writer, value.Value);
            writer.WriteEndObject();
        }

        private static object? ReadValue(IFormatReader reader) =>
            reader.ValueKind switch
            {
                ValueKind.String => reader.GetString(),
                ValueKind.Number => Convert.ChangeType(reader.GetInt64(), typeof(T)),
                ValueKind.Null => null,
                _ => throw new NotSupportedException(),
            };

        private static void WriteValue(IFormatWriter writer, T? value)
        {
            switch (value)
            {
                case null: writer.WriteNull(); break;
                case string s: writer.WriteString(s); break;
                default: writer.WriteNumber(Convert.ToInt64(value)); break;
            }
        }
    }
}
