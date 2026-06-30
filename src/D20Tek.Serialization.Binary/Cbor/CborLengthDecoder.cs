using System.Buffers.Binary;

namespace D20Tek.Serialization.Binary.Cbor;

/// <summary>
/// Pure-function utilities for decoding CBOR length headers. These helpers operate on raw
/// byte spans and have no dependency on reader state, making them independently testable.
/// </summary>
internal static class CborLengthDecoder
{
    /// <summary>
    /// Decodes the length header of a CBOR text or byte string and returns the payload's byte
    /// offset and length within <paramref name="encoded"/>.
    /// </summary>
    /// <param name="encoded">The full encoded string item, beginning with its initial byte.</param>
    /// <returns>The payload's offset and length within <paramref name="encoded"/>.</returns>
    /// <exception cref="SerializationException">Thrown when the length header is malformed.</exception>
    internal static (int Offset, int Length) GetStringPayloadRange(ReadOnlySpan<byte> encoded)
    {
        int additionalInfo = encoded[0] & 0x1F;
        return additionalInfo switch
        {
            <= 23 => (1, additionalInfo),
            24 => (2, encoded[1]),
            25 => (3, BinaryPrimitives.ReadUInt16BigEndian(encoded.Slice(1, 2))),
            26 => (5, checked((int)BinaryPrimitives.ReadUInt32BigEndian(encoded.Slice(1, 4)))),
            27 => (9, checked((int)BinaryPrimitives.ReadUInt64BigEndian(encoded.Slice(1, 8)))),
            _ => throw new SerializationException("Malformed CBOR string length header.", "$"),
        };
    }
}
