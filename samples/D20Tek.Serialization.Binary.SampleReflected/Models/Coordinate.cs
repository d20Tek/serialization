namespace D20Tek.Serialization.Binary.SampleReflected.Models;

/// <summary>
/// Sample value type (struct) serialized via reflection.
/// </summary>
public struct Coordinate
{
    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public float? Altitude { get; set; }
}
