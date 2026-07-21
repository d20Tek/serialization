using D20Tek.Serialization;

namespace D20Tek.Serialization.Binary.PackageValidation.Models;

/// <summary>
/// Struct model — validates source generator handles value types from the published package.
/// </summary>
[Serializable]
public struct Location
{
    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public float? Elevation { get; set; }
}
