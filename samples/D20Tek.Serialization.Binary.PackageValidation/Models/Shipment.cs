using D20Tek.Serialization;

namespace D20Tek.Serialization.Binary.PackageValidation.Models;

/// <summary>
/// Enum for shipment status — validates EnumCborConverter from the published package.
/// </summary>
public enum ShipmentStatus
{
    Pending,
    InTransit,
    Delivered,
    Returned,
}

/// <summary>
/// Shipment model with enum and nullable members — validates full attribute set
/// from the published package's source generator.
/// </summary>
[Serializable]
public sealed class Shipment
{
    [RequiredSerialized]
    public int ShipmentId { get; set; }

    [RequiredSerialized]
    public int ProductId { get; set; }

    public ShipmentStatus Status { get; set; }

    public double? Weight { get; set; }

    public ShipmentStatus? PreviousStatus { get; set; }
}
