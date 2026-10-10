namespace D20Tek.Serialization.Binary.SampleReflected.Models;

/// <summary>
/// Sample polymorphic base type, serialized via the reflection fallback (no
/// <see cref="SerializableAttribute"/>). Demonstrates that <c>BinarySerializer</c> writes and
/// reads a type discriminator so the concrete derived type can be recovered on deserialization.
/// </summary>
[Polymorphic]
[SerializedDerivedType(typeof(Dog), "dog")]
[SerializedDerivedType(typeof(Cat), "cat")]
public abstract class Animal
{
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// A derived type registered under the "dog" discriminator.
/// </summary>
public sealed class Dog : Animal
{
    public string Breed { get; set; } = string.Empty;
}

/// <summary>
/// A derived type registered under the "cat" discriminator.
/// </summary>
public sealed class Cat : Animal
{
    public int LivesLeft { get; set; }
}

/// <summary>
/// A container whose <see cref="Pet"/> member is declared with the polymorphic base type.
/// </summary>
public sealed class Owner
{
    public string Name { get; set; } = string.Empty;

    public Animal? Pet { get; set; }
}
