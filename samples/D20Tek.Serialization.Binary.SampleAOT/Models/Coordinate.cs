using D20Tek.Serialization;

namespace D20Tek.Serialization.Binary.SampleAOT.Models;

/// <summary>
/// Sample value type (struct) validating that the generator produces correct
/// serializers for structs (non-nullable Read return type, no null guards in Write).
/// </summary>
[Serializable]
public struct Coordinate
{
    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public float? Altitude { get; set; }
}
