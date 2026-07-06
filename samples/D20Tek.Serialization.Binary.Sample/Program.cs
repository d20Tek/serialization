// D20Tek.Serialization.Binary sample application.
//
// This sample is the AOT publish validation target for the platform (task 4.3.3).
// It exercises the source-generated serialization path, the BinaryDocument DOM, and
// built-in converter round-trips — all AOT-safe with no reflection fallback.

using D20Tek.Serialization;
using D20Tek.Serialization.Binary.Sample.Models;
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
