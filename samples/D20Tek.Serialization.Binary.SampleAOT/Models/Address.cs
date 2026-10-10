namespace D20Tek.Serialization.Binary.SampleAOT.Models;

/// <summary>
/// Sample positional record validating that the generator produces constructor-based
/// construction for types without an accessible parameterless constructor, combined with an
/// init-only property that is not part of the primary constructor.
/// </summary>
[Serializable]
public sealed record Address(string Street, string City, string PostalCode)
{
    public string? Country { get; init; }
}
