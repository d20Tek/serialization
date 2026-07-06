using D20Tek.Serialization;

namespace D20Tek.Serialization.Binary.SampleAOT.Models;

/// <summary>
/// Sample class exercising the source generator with multiple member strategies,
/// nullable types, naming overrides, and required members.
/// </summary>
[Serializable]
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
