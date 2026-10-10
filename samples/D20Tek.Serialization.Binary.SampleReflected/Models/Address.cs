namespace D20Tek.Serialization.Binary.SampleReflected.Models;

/// <summary>
/// Sample positional record without the <see cref="SerializableAttribute"/>, so
/// <c>BinarySerializer</c> resolves to the reflection fallback. Validates that the reflection
/// path resolves constructor-based construction for types without an accessible parameterless
/// constructor, combined with an init-only property that is not part of the primary constructor.
/// </summary>
public sealed record Address(string Street, string City, string PostalCode)
{
    public string? Country { get; init; }
}
