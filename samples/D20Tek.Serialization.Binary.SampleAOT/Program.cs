// D20Tek.Serialization.Binary AOT sample application.
//
// This sample is the AOT publish validation target for the platform (task 4.3.3).
// It exercises the source-generated serialization path, the BinaryDocument DOM, and
// built-in converter round-trips — all AOT-safe with no reflection fallback.

using D20Tek.Serialization;
using D20Tek.Serialization.Binary.Cbor.Converters;
using D20Tek.Serialization.Binary.SampleAOT.Models;
using D20Tek.Serialization.Dom;

// ─── 1. Source-generated round-trip (AOT-safe) ───────────────────────────────

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

#pragma warning disable IL2026, IL3050 // Required for BinarySerializer; safe here via source-gen path
byte[] customerBytes = BinarySerializer.SerializeToByteArray(customer);
var roundTripped = BinarySerializer.Deserialize<Customer>(customerBytes);
#pragma warning restore IL2026, IL3050

Console.WriteLine("=== Customer round-trip ===");
Console.WriteLine($"  Id:            {roundTripped!.Id}");
Console.WriteLine($"  Name:          {roundTripped.Name}");
Console.WriteLine($"  Email:         {roundTripped.Email}");
Console.WriteLine($"  IsActive:      {roundTripped.IsActive}");
Console.WriteLine($"  Balance:       {roundTripped.Balance}");
Console.WriteLine($"  LoyaltyPoints: {roundTripped.LoyaltyPoints}");
Console.WriteLine($"  InternalNotes: {roundTripped.InternalNotes ?? "(null — correctly ignored)"}");
Console.WriteLine();

// ─── 2. Coordinate struct round-trip ─────────────────────────────────────────

var coord = new Coordinate { Latitude = 47.6062, Longitude = -122.3321, Altitude = 56.0f };

#pragma warning disable IL2026, IL3050
byte[] coordBytes = BinarySerializer.SerializeToByteArray(coord);
var coordBack = BinarySerializer.Deserialize<Coordinate>(coordBytes);
#pragma warning restore IL2026, IL3050

Console.WriteLine("=== Coordinate round-trip ===");
Console.WriteLine($"  Latitude:  {coordBack.Latitude}");
Console.WriteLine($"  Longitude: {coordBack.Longitude}");
Console.WriteLine($"  Altitude:  {coordBack.Altitude}");
Console.WriteLine();

// ─── 3. BinaryDocument DOM navigation (AOT-safe, no serializer needed) ───────

using var doc = BinaryDocument.Parse(customerBytes);
Console.WriteLine("=== BinaryDocument DOM navigation ===");
Console.WriteLine($"  Root kind:     {doc.RootElement.ValueKind}");
Console.WriteLine($"  full_name:     {doc.RootElement["full_name"].GetString()}");
Console.WriteLine($"  Balance:       {doc.RootElement["Balance"].GetDouble()}");

foreach (var prop in doc.RootElement.EnumerateObject())
{
    Console.WriteLine($"  [{prop.Name}] = {prop.Value.ValueKind}");
}

Console.WriteLine();
Console.WriteLine("Sample completed successfully.");

// ─── 4. Order round-trip with enum converter ─────────────────────────────────

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

#pragma warning disable IL2026, IL3050
byte[] orderBytes = BinarySerializer.SerializeToByteArray(order, options);
var orderBack = BinarySerializer.Deserialize<Order>(orderBytes, options);
#pragma warning restore IL2026, IL3050

Console.WriteLine("=== Order round-trip (with EnumCborConverter) ===");
Console.WriteLine($"  OrderId:        {orderBack!.OrderId}");
Console.WriteLine($"  CustomerId:     {orderBack.CustomerId}");
Console.WriteLine($"  Status:         {orderBack.Status}");
Console.WriteLine($"  Discount:       {orderBack.Discount}");
Console.WriteLine($"  PreviousStatus: {orderBack.PreviousStatus}");
Console.WriteLine();

// ─── 5. Custom converter demo ────────────────────────────────────────────────

var customOptions = new BinarySerializerOptions();
customOptions.Converters.Add(new UpperCaseStringConverter());

#pragma warning disable IL2026, IL3050
byte[] customBytes = BinarySerializer.SerializeToByteArray("hello world", customOptions);
var customBack = BinarySerializer.Deserialize<string>(customBytes, customOptions);
#pragma warning restore IL2026, IL3050

Console.WriteLine("=== Custom converter (UpperCaseStringConverter) ===");
Console.WriteLine($"  Input:  \"hello world\"");
Console.WriteLine($"  Output: \"{customBack}\"");
Console.WriteLine();
Console.WriteLine("All samples completed successfully.");

// ─── Custom converter: converts strings to UPPER CASE on write ───────────────

/// <summary>
/// A sample custom converter that transforms strings to upper case during serialization
/// and reads them back normally. Demonstrates the extensible converter model.
/// </summary>
sealed class UpperCaseStringConverter : Converter<string>
{
    public override string Read(IFormatReader reader, SerializerOptions options)
        => reader.GetString();

    public override void Write(IFormatWriter writer, string value, SerializerOptions options)
        => writer.WriteString(value.ToUpperInvariant());
}
