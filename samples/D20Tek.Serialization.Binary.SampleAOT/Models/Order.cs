using D20Tek.Serialization;

namespace D20Tek.Serialization.Binary.SampleAOT.Models;

/// <summary>
/// Sample enum consumed by <see cref="Order"/> to validate enum serialization strategy.
/// </summary>
public enum OrderStatus
{
    Pending,
    Shipped,
    Delivered,
    Cancelled,
}

/// <summary>
/// Sample class exercising enum member strategy, nullable value types, and multiple
/// required members.
/// </summary>
[Serializable]
public sealed class Order
{
    [RequiredSerialized]
    public int OrderId { get; set; }

    [RequiredSerialized]
    public int CustomerId { get; set; }

    public OrderStatus Status { get; set; }

    public double? Discount { get; set; }

    public OrderStatus? PreviousStatus { get; set; }
}
