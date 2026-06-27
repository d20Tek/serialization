using D20Tek.Serialization.Core.Tests.Fakes;
using D20Tek.Serialization.Generation;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Core.Tests;

[TestClass]
public sealed class GeneratedSerializerContractTests
{
    private static readonly string[] expected =
    [
        "StartObject",
        "Property:name",
        "String:Alice",
        "Property:age",
        "Number:30",
        "EndObject",
    ];

    [TestMethod]
    public void Serializer_Write_EmitsExpectedTokens()
    {
        // arrange
        var serializer = new FakePersonSerializer();
        var writer = new RecordingFormatWriter();
        var options = new TestOptions();

        // act
        serializer.Write(writer, new Person { Name = "Alice", Age = 30 }, options);

        // assert
        CollectionAssert.AreEqual(expected, writer.Tokens);
    }

    [TestMethod]
    public void Serializer_Read_ReturnsReconstructedValue()
    {
        // arrange
        var serializer = new FakePersonSerializer();
        var reader = new ScriptedFormatReader("Bob", 42);
        var options = new TestOptions();

        // act
        var result = serializer.Read(reader, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Bob", result.Name);
        Assert.AreEqual(42, result.Age);
    }

    [TestMethod]
    public void Serializer_RoundTrips_ThroughContract()
    {
        // arrange
        IGeneratedSerializer<Person> serializer = new FakePersonSerializer();
        var options = new TestOptions();
        var original = new Person { Name = "Carol", Age = 19 };

        // act
        var reader = new ScriptedFormatReader(original.Name, original.Age);
        var clone = serializer.Read(reader, options);

        // assert
        Assert.IsNotNull(clone);
        Assert.AreEqual(original.Name, clone.Name);
        Assert.AreEqual(original.Age, clone.Age);
    }

    [TestMethod]
    public void Registry_TryGetSerializer_ReturnsTrueForRegisteredType()
    {
        // arrange
        var expected = new FakePersonSerializer();
        IGeneratedSerializerRegistry registry = new FakeRegistry(expected);

        // act
        var found = registry.TryGetSerializer<Person>(out var serializer);

        // assert
        Assert.IsTrue(found);
        Assert.AreSame(expected, serializer);
    }

    [TestMethod]
    public void Registry_TryGetSerializer_ReturnsFalseForUnregisteredType()
    {
        // arrange
        IGeneratedSerializerRegistry registry = new FakeRegistry(new FakePersonSerializer());

        // act
        var found = registry.TryGetSerializer<Address>(out var serializer);

        // assert
        Assert.IsFalse(found);
        Assert.IsNull(serializer);
    }

    private sealed class TestOptions : SerializerOptions { }

    private sealed class Person
    {
        public string? Name { get; set; }

        public int Age { get; set; }
    }

    private sealed class Address
    {
        public string? Street { get; set; }
    }

    private sealed class FakePersonSerializer : IGeneratedSerializer<Person>
    {
        [ExcludeFromCodeCoverage]
        public void Write(IFormatWriter writer, Person value, SerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("name");
            writer.WriteString(value.Name ?? string.Empty);
            writer.WritePropertyName("age");
            writer.WriteNumber(value.Age);
            writer.WriteEndObject();
        }

        public Person? Read(IFormatReader reader, SerializerOptions options)
        {
            var person = new Person();
            reader.ReadStartObject();
            while (reader.TryReadPropertyName(out var name))
            {
                switch (name)
                {
                    case "name":
                        person.Name = reader.GetString();
                        break;
                    case "age":
                        person.Age = (int)reader.GetInt64();
                        break;
                }
            }

            reader.ReadEndObject();
            return person;
        }
    }

    private sealed class FakeRegistry(IGeneratedSerializer<Person> personSerializer) : IGeneratedSerializerRegistry
    {
        public bool TryGetSerializer<T>(out IGeneratedSerializer<T> serializer)
        {
            if (personSerializer is IGeneratedSerializer<T> typed)
            {
                serializer = typed;
                return true;
            }

            serializer = null!;
            return false;
        }
    }
}
