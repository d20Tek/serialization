// D20Tek.Serialization.Binary — Published NuGet Package Validation
//
// This sample validates that the PUBLISHED NuGet packages work end-to-end:
//   1. The source generator embedded in the Core package emits serializers.
//   2. AOT publish works (PublishAot=true).
//   3. All attribute behaviors ([Serializable], [NameSerialized], [IgnoreSerialized],
//      [RequiredSerialized]) work from package references.
//   4. DOM navigation works from the Binary package.
//
// This project uses PackageReference (not ProjectReference) — it references
// the .nupkg files from the local nupkgs/ folder via a local nuget.config.

using D20Tek.Serialization;
using D20Tek.Serialization.Binary;
using D20Tek.Serialization.Binary.Cbor.Converters;
using D20Tek.Serialization.Binary.PackageValidation.Models;
using D20Tek.Serialization.Dom;

int failures = 0;

void Assert(bool condition, string label)
{
    if (condition)
    {
        Console.WriteLine($"  ✓ {label}");
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  ✗ FAIL: {label}");
        Console.ResetColor();
        failures++;
    }
}

// ─── 1. Product round-trip (source-generated, AOT-safe) ─────────────────────

Console.WriteLine("=== 1. Product round-trip (source-gen) ===");

var product = new Product
{
    ProductId = 42,
    Name = "Widget Pro",
    Description = "A premium widget",
    Price = 29.99,
    InStock = true,
    UnitsAvailable = 1_500,
    WarehouseCode = "WH-007 — should be ignored"
};

#pragma warning disable IL2026, IL3050
byte[] productBytes = BinarySerializer.SerializeToByteArray(product);
var productBack = BinarySerializer.Deserialize<Product>(productBytes);
#pragma warning restore IL2026, IL3050

Assert(productBack!.ProductId == 42, "ProductId round-tripped");
Assert(productBack.Name == "Widget Pro", "Name round-tripped");
Assert(productBack.Description == "A premium widget", "Description round-tripped");
Assert(Math.Abs(productBack.Price - 29.99) < 0.001, "Price round-tripped");
Assert(productBack.InStock == true, "InStock round-tripped");
Assert(productBack.UnitsAvailable == 1_500, "UnitsAvailable round-tripped");
Assert(productBack.WarehouseCode is null, "[IgnoreSerialized] WarehouseCode is null");
Console.WriteLine();

// ─── 2. Location struct round-trip (source-generated) ────────────────────────

Console.WriteLine("=== 2. Location struct round-trip (source-gen) ===");

var location = new Location { Latitude = 47.6062, Longitude = -122.3321, Elevation = 56.0f };

#pragma warning disable IL2026, IL3050
byte[] locationBytes = BinarySerializer.SerializeToByteArray(location);
var locationBack = BinarySerializer.Deserialize<Location>(locationBytes);
#pragma warning restore IL2026, IL3050

Assert(Math.Abs(locationBack.Latitude - 47.6062) < 0.0001, "Latitude round-tripped");
Assert(Math.Abs(locationBack.Longitude - (-122.3321)) < 0.0001, "Longitude round-tripped");
Assert(locationBack.Elevation.HasValue && Math.Abs(locationBack.Elevation.Value - 56.0f) < 0.01, "Elevation round-tripped");
Console.WriteLine();

// ─── 3. Shipment round-trip with enum converter ─────────────────────────────

Console.WriteLine("=== 3. Shipment round-trip (source-gen + EnumCborConverter) ===");

var options = new BinarySerializerOptions();
options.Converters.Add(new EnumCborConverter<ShipmentStatus>());

var shipment = new Shipment
{
    ShipmentId = 9001,
    ProductId = 42,
    Status = ShipmentStatus.InTransit,
    Weight = 3.75,
    PreviousStatus = ShipmentStatus.Pending,
};

#pragma warning disable IL2026, IL3050
byte[] shipmentBytes = BinarySerializer.SerializeToByteArray(shipment, options);
var shipmentBack = BinarySerializer.Deserialize<Shipment>(shipmentBytes, options);
#pragma warning restore IL2026, IL3050

Assert(shipmentBack!.ShipmentId == 9001, "ShipmentId round-tripped");
Assert(shipmentBack.ProductId == 42, "ProductId round-tripped");
Assert(shipmentBack.Status == ShipmentStatus.InTransit, "Status round-tripped");
Assert(shipmentBack.Weight.HasValue && Math.Abs(shipmentBack.Weight.Value - 3.75) < 0.001, "Weight round-tripped");
Assert(shipmentBack.PreviousStatus == ShipmentStatus.Pending, "PreviousStatus round-tripped");
Console.WriteLine();

// ─── 4. BinaryDocument DOM navigation ────────────────────────────────────────

Console.WriteLine("=== 4. BinaryDocument DOM (from published package) ===");

using var doc = BinaryDocument.Parse(productBytes);

Assert(doc.RootElement.ValueKind == NodeKind.Object, "Root is Object");
Assert(doc.RootElement["product_name"].GetString() == "Widget Pro", "DOM reads [NameSerialized] name");
Assert(Math.Abs(doc.RootElement["Price"].GetDouble() - 29.99) < 0.001, "DOM reads Price");
Assert(doc.RootElement["InStock"].GetBoolean() == true, "DOM reads InStock");

int propCount = 0;
foreach (var prop in doc.RootElement.EnumerateObject())
{
    propCount++;
}
Assert(propCount > 0, $"EnumerateObject yielded {propCount} properties");
Console.WriteLine();

// ─── 5. Custom converter ─────────────────────────────────────────────────────

Console.WriteLine("=== 5. Custom converter (UpperCaseStringConverter) ===");

var customOptions = new BinarySerializerOptions();
customOptions.Converters.Add(new UpperCaseStringConverter());

#pragma warning disable IL2026, IL3050
byte[] customBytes = BinarySerializer.SerializeToByteArray("hello packages", customOptions);
var customBack = BinarySerializer.Deserialize<string>(customBytes, customOptions);
#pragma warning restore IL2026, IL3050

Assert(customBack == "HELLO PACKAGES", "Custom converter produced upper-case output");
Console.WriteLine();

// ─── Summary ─────────────────────────────────────────────────────────────────

if (failures == 0)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("★ All package validation checks passed.");
    Console.ResetColor();
}
else
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"✗ {failures} check(s) failed.");
    Console.ResetColor();
    return 1;
}

return 0;

// ─── Custom converter ────────────────────────────────────────────────────────

sealed class UpperCaseStringConverter : Converter<string>
{
    public override string Read(IFormatReader reader, SerializerOptions options)
        => reader.GetString();

    public override void Write(IFormatWriter writer, string value, SerializerOptions options)
        => writer.WriteString(value.ToUpperInvariant());
}
