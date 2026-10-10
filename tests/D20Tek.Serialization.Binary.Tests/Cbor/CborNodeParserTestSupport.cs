using System.Formats.Cbor;

namespace D20Tek.Serialization.Binary.Tests.Cbor;

/// <summary>
/// Shared CBOR encoding helper used by the split <c>CborNodeParser*Tests</c> classes.
/// </summary>
internal static class CborNodeParserTestSupport
{
    internal static byte[] Encode(Action<CborWriter> write)
    {
        var writer = new CborWriter(CborConformanceMode.Lax, convertIndefiniteLengthEncodings: true);
        write(writer);
        return writer.Encode();
    }
}
