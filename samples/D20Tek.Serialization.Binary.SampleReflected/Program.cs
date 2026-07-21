// D20Tek.Serialization.Binary reflection-based sample application.
//
// This sample demonstrates the reflection fallback serialization path.
// The model types do NOT carry [Serializable], so BinarySerializer resolves
// to the ReflectionBinarySerializer at runtime.

using D20Tek.Serialization;
using D20Tek.Serialization.Binary;
using D20Tek.Serialization.Binary.Cbor.Converters;
using D20Tek.Serialization.Binary.SampleReflected.Models;

// ─── 1. Customer round-trip (reflection path) ────────────────────────────────

var customer = new Customer
{
    Id = 1,
    Name = "Alice Johnson",
    Email = "alice@example.com",
    IsActive = true,
    Balance = 1234.56,
    LoyaltyPoints = 50_000,
    InternalNotes = "VIP — should be ignored by serialization"
};

byte[] customerBytes = BinarySerializer.SerializeToByteArray(customer);
var roundTripped = BinarySerializer.Deserialize<Customer>(customerBytes);

Console.WriteLine("=== Customer round-trip (reflection) ===");
Console.WriteLine($"  Id:            {roundTripped!.Id}");
Console.WriteLine($"  Name:          {roundTripped.Name}");
Console.WriteLine($"  Email:         {roundTripped.Email}");
Console.WriteLine($"  IsActive:      {roundTripped.IsActive}");
Console.WriteLine($"  Balance:       {roundTripped.Balance}");
Console.WriteLine($"  LoyaltyPoints: {roundTripped.LoyaltyPoints}");
Console.WriteLine($"  InternalNotes: {roundTripped.InternalNotes ?? "(null — correctly ignored)"}");
Console.WriteLine();

// ─── 2. Coordinate struct round-trip (reflection path) ───────────────────────

var coord = new Coordinate { Latitude = 47.6062, Longitude = -122.3321, Altitude = 56.0f };

byte[] coordBytes = BinarySerializer.SerializeToByteArray(coord);
var coordBack = BinarySerializer.Deserialize<Coordinate>(coordBytes);

Console.WriteLine("=== Coordinate round-trip (reflection) ===");
Console.WriteLine($"  Latitude:  {coordBack.Latitude}");
Console.WriteLine($"  Longitude: {coordBack.Longitude}");
Console.WriteLine($"  Altitude:  {coordBack.Altitude}");
Console.WriteLine();

// ─── 3. Order round-trip with enum converter (reflection path) ───────────────

var options = new BinarySerializerOptions();
options.Converters.Add(new EnumCborConverter<OrderStatus>());

var order = new Order
{
    OrderId = 1001,
    CustomerId = 1,
    Status = OrderStatus.Shipped,
    Discount = 10.5,
    PreviousStatus = OrderStatus.Pending,
};

byte[] orderBytes = BinarySerializer.SerializeToByteArray(order, options);
var orderBack = BinarySerializer.Deserialize<Order>(orderBytes, options);

Console.WriteLine("=== Order round-trip (reflection, with EnumCborConverter) ===");
Console.WriteLine($"  OrderId:        {orderBack!.OrderId}");
Console.WriteLine($"  CustomerId:     {orderBack.CustomerId}");
Console.WriteLine($"  Status:         {orderBack.Status}");
Console.WriteLine($"  Discount:       {orderBack.Discount}");
Console.WriteLine($"  PreviousStatus: {orderBack.PreviousStatus}");
Console.WriteLine();

// ─── 4. Custom converter demo (reflection path) ─────────────────────────────

var customOptions = new BinarySerializerOptions();
customOptions.Converters.Add(new UpperCaseStringConverter());

byte[] customBytes = BinarySerializer.SerializeToByteArray("hello world", customOptions);
var customBack = BinarySerializer.Deserialize<string>(customBytes, customOptions);

Console.WriteLine("=== Custom converter (UpperCaseStringConverter) ===");
Console.WriteLine($"  Input:  \"hello world\"");
Console.WriteLine($"  Output: \"{customBack}\"");
Console.WriteLine();

// ─── 5. Camel-case naming policy demo ────────────────────────────────────────

var camelOptions = new BinarySerializerOptions
{
    PropertyNamingPolicy = NamingPolicy.CamelCase
};

byte[] camelBytes = BinarySerializer.SerializeToByteArray(customer, camelOptions);
var camelBack = BinarySerializer.Deserialize<Customer>(camelBytes, camelOptions);

Console.WriteLine("=== Camel-case naming policy (reflection) ===");
Console.WriteLine($"  Name:    {camelBack!.Name}");
Console.WriteLine($"  Email:   {camelBack.Email}");
Console.WriteLine($"  Balance: {camelBack.Balance}");
Console.WriteLine();

Console.WriteLine("All reflection-based samples completed successfully.");

// ─── Custom converter: converts strings to UPPER CASE on write ───────────────

/// <summary>
/// A sample custom converter that transforms strings to upper case during serialization.
/// </summary>
sealed class UpperCaseStringConverter : Converter<string>
{
    public override string Read(IFormatReader reader, SerializerOptions options)
        => reader.GetString();

    public override void Write(IFormatWriter writer, string value, SerializerOptions options)
        => writer.WriteString(value.ToUpperInvariant());
}
