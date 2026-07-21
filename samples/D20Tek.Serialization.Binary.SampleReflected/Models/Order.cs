namespace D20Tek.Serialization.Binary.SampleReflected.Models;

/// <summary>
/// Sample enum consumed by <see cref="Order"/>.
/// </summary>
public enum OrderStatus
{
    Pending,
    Shipped,
    Delivered,
    Cancelled,
}

/// <summary>
/// Sample class with enum members and nullable value types, serialized via reflection.
/// </summary>
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
