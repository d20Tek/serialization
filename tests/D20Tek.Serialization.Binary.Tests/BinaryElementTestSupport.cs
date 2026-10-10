using System.Formats.Cbor;

namespace D20Tek.Serialization.Binary.Tests;

/// <summary>
/// Shared CBOR parsing helper used by the split <c>BinaryElement*Tests</c> classes.
/// </summary>
internal static class BinaryElementTestSupport
{
    internal static BinaryDocument ParseCbor(Action<CborWriter> write)
    {
        var writer = new CborWriter(CborConformanceMode.Lax, convertIndefiniteLengthEncodings: true);
        write(writer);
        return BinaryDocument.Parse(writer.Encode());
    }
}
