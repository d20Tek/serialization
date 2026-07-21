using D20Tek.Serialization;

namespace D20Tek.Serialization.Binary.PackageValidation.Models;

/// <summary>
/// Product model — validates that the source generator inside the published
/// Core NuGet package correctly emits a serializer for this type.
/// </summary>
[Serializable]
public sealed class Product
{
    [RequiredSerialized]
    public int ProductId { get; set; }

    [NameSerialized("product_name")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public double Price { get; set; }

    public bool InStock { get; set; }

    public long UnitsAvailable { get; set; }

    [IgnoreSerialized]
    public string? WarehouseCode { get; set; }
}
