namespace D20Tek.Serialization.Binary.SampleReflected.Models;

/// <summary>
/// Sample class using reflection-based serialization (no [Serializable] attribute).
/// Demonstrates naming overrides, ignore, and required via serialization attributes
/// that are still honored by the reflection path.
/// </summary>
public sealed class Customer
{
    [RequiredSerialized]
    public int Id { get; set; }

    [NameSerialized("full_name")]
    public string Name { get; set; } = string.Empty;

    public string? Email { get; set; }

    public bool IsActive { get; set; }

    public double Balance { get; set; }

    public long LoyaltyPoints { get; set; }

    [IgnoreSerialized]
    public string? InternalNotes { get; set; }
}
