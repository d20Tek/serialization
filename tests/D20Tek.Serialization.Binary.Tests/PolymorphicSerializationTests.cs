using D20Tek.Serialization.Binary.Cbor;
using D20Tek.Serialization.Binary.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
[SuppressMessage("Trimming", "IL2026")]
[SuppressMessage("AOT", "IL3050")]
public sealed class PolymorphicSerializationTests
{
    private readonly ReflectionBinarySerializer _serializer = new();

    // --- Round-trip: polymorphic member, concrete derived types ---

    [TestMethod]
    public void RoundTrip_PolymorphicMember_Dog_PreservesDerivedTypeAndMembers()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var original = new Owner { Name = "Alice", Pet = new Dog { Name = "Rex", Breed = "Husky" } };

        // act
        var bytes = Serialize(original, options);
        var result = Deserialize<Owner>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Alice", result.Name);
        var dog = Assert.IsInstanceOfType<Dog>(result.Pet);
        Assert.AreEqual("Rex", dog.Name);
        Assert.AreEqual("Husky", dog.Breed);
    }

    [TestMethod]
    public void RoundTrip_PolymorphicMember_Cat_PreservesDerivedTypeAndMembers()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var original = new Owner { Name = "Bob", Pet = new Cat { Name = "Whiskers", LivesLeft = 9 } };

        // act
        var bytes = Serialize(original, options);
        var result = Deserialize<Owner>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Bob", result.Name);
        var cat = Assert.IsInstanceOfType<Cat>(result.Pet);
        Assert.AreEqual("Whiskers", cat.Name);
        Assert.AreEqual(9, cat.LivesLeft);
    }

    [TestMethod]
    public void RoundTrip_PolymorphicMember_Null_PreservesNull()
    {
        // arrange
        var options = new BinarySerializerOptions();
        var original = new Owner { Name = "Carol", Pet = null };

        // act
        var bytes = Serialize(original, options);
        var result = Deserialize<Owner>(bytes, options);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Carol", result.Name);
        Assert.IsNull(result.Pet);
    }

    [TestMethod]
    public void RoundTrip_PolymorphicRootValue_PreservesDerivedType()
    {
        // arrange
        var options = new BinarySerializerOptions();
        Animal original = new Dog { Name = "Fido", Breed = "Beagle" };

        // act
        var bytes = Serialize(original, options);
        var result = Deserialize<Animal>(bytes, options);

        // assert
        var dog = Assert.IsInstanceOfType<Dog>(result);
        Assert.AreEqual("Fido", dog.Name);
        Assert.AreEqual("Beagle", dog.Breed);
    }

    // --- Custom discriminator property name ---

    [TestMethod]
    public void RoundTrip_CustomDiscriminatorPropertyName_PreservesDerivedType()
    {
        // arrange
        var options = new BinarySerializerOptions();
        Shape original = new Circle { Radius = 2.5 };

        // act
        var bytes = Serialize(original, options);
        var result = Deserialize<Shape>(bytes, options);

        // assert
        var circle = Assert.IsInstanceOfType<Circle>(result);
        Assert.AreEqual(2.5, circle.Radius);
    }

    // --- Error cases ---

    [TestMethod]
    public void Write_UnregisteredDerivedType_ThrowsSerializationException()
    {
        // arrange
        var options = new BinarySerializerOptions();
        Animal original = new Bird { Name = "Tweety" };

        // act & assert
        Assert.ThrowsExactly<SerializationException>(
            [ExcludeFromCodeCoverage] () => Serialize(original, options));
    }

    [TestMethod]
    public void Read_UnknownDiscriminator_ThrowsSerializationException()
    {
        // arrange — manually build CBOR with an unregistered discriminator
        var options = new BinarySerializerOptions();
        var writer = new CborFormatWriter();
        writer.WriteStartObject();
        writer.WritePropertyName("$type");
        writer.WriteString("fish");
        writer.WritePropertyName("Name");
        writer.WriteString("Nemo");
        writer.WriteEndObject();
        var data = writer.Encode();

        // act & assert
        Assert.ThrowsExactly<SerializationException>(
            [ExcludeFromCodeCoverage] () => Deserialize<Animal>(data, options));
    }

    [TestMethod]
    public void Read_MissingDiscriminatorAsFirstProperty_ThrowsSerializationException()
    {
        // arrange — discriminator property is not first
        var options = new BinarySerializerOptions();
        var writer = new CborFormatWriter();
        writer.WriteStartObject();
        writer.WritePropertyName("Name");
        writer.WriteString("Rex");
        writer.WritePropertyName("$type");
        writer.WriteString("dog");
        writer.WriteEndObject();
        var data = writer.Encode();

        // act & assert
        Assert.ThrowsExactly<SerializationException>(
            [ExcludeFromCodeCoverage] () => Deserialize<Animal>(data, options));
    }

    [TestMethod]
    public void Read_EmptyObject_NoDiscriminatorProperty_ThrowsSerializationException()
    {
        // arrange — object has no properties at all, so a property name cannot be read
        var options = new BinarySerializerOptions();
        var writer = new CborFormatWriter();
        writer.WriteStartObject();
        writer.WriteEndObject();
        var data = writer.Encode();

        // act & assert
        Assert.ThrowsExactly<SerializationException>(
            [ExcludeFromCodeCoverage] () => Deserialize<Animal>(data, options));
    }

    // --- Helpers ---

    private byte[] Serialize<T>(T value, BinarySerializerOptions options)
    {
        var writer = new CborFormatWriter();
        _serializer.Write(writer, value, options);
        return writer.Encode();
    }

    private T? Deserialize<T>(byte[] bytes, BinarySerializerOptions options)
    {
        var reader = new CborFormatReader(bytes);
        return _serializer.Read<T>(reader, options);
    }

    // --- Test models ---

    [Polymorphic]
    [SerializedDerivedType(typeof(Dog), "dog")]
    [SerializedDerivedType(typeof(Cat), "cat")]
    public abstract class Animal
    {
        public string? Name { get; set; }
    }

    public sealed class Dog : Animal
    {
        public string? Breed { get; set; }
    }

    public sealed class Cat : Animal
    {
        public int LivesLeft { get; set; }
    }

    // Intentionally NOT registered via [SerializedDerivedType] on Animal.
    public sealed class Bird : Animal
    {
    }

    public sealed class Owner
    {
        public string? Name { get; set; }
        public Animal? Pet { get; set; }
    }

    [Polymorphic(TypeDiscriminatorPropertyName = "kind")]
    [SerializedDerivedType(typeof(Circle), "circle")]
    public abstract class Shape
    {
    }

    public sealed class Circle : Shape
    {
        public double Radius { get; set; }
    }
}
