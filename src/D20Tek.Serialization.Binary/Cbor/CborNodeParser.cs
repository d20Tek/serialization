using D20Tek.Serialization.Binary.Cbor.Converters;
using D20Tek.Serialization.Dom;
using System.Formats.Cbor;

namespace D20Tek.Serialization.Binary.Cbor;

/// <summary>
/// Parses raw CBOR bytes into the shared <see cref="Node"/> tree used by the DOM
/// (<see cref="Dom"/>). Maps, arrays, and scalars are converted to their corresponding
/// <see cref="NodeKind"/> values. Known CBOR semantic tags are decoded into meaningful
/// string representations:
/// <list type="bullet">
///   <item><description>Tag 37 (UUID) → <see cref="NodeKind.String"/> with the GUID formatted as a standard string.</description></item>
///   <item><description>Tag 1 (epoch date/time) → <see cref="NodeKind.String"/> with the UTC date/time in round-trip format ("O").</description></item>
/// </list>
/// Unknown tags are consumed and the data item is parsed normally.
/// </summary>
internal static class CborNodeParser
{
    private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Parses the supplied CBOR-encoded data into a <see cref="Node"/> tree.
    /// </summary>
    /// <param name="data">The CBOR-encoded document.</param>
    /// <returns>The root <see cref="Node"/> of the parsed tree.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when an indefinite-length container or an unrecognized CBOR state is encountered.
    /// </exception>
    internal static Node Parse(ReadOnlyMemory<byte> data)
    {
        var reader = new CborReader(data, CborConformanceMode.Lax, allowMultipleRootLevelValues: false);
        return ReadNode(reader);
    }

    private static Node ReadNode(CborReader reader)
    {
        var state = reader.PeekState();

        return state switch
        {
            CborReaderState.Null => ReadNull(reader),
            CborReaderState.Boolean => Node.CreateBoolean(reader.ReadBoolean()),
            CborReaderState.UnsignedInteger or CborReaderState.NegativeInteger =>
                Node.CreateNumber(reader.ReadInt64()),
            CborReaderState.HalfPrecisionFloat => Node.CreateNumber((double)reader.ReadHalf()),
            CborReaderState.SinglePrecisionFloat => Node.CreateNumber(reader.ReadSingle()),
            CborReaderState.DoublePrecisionFloat => Node.CreateNumber(reader.ReadDouble()),
            CborReaderState.TextString => Node.CreateString(reader.ReadTextString()),
            CborReaderState.ByteString => Node.CreateString(Convert.ToBase64String(reader.ReadByteString())),
            CborReaderState.StartMap => ReadMap(reader),
            CborReaderState.StartArray => ReadArray(reader),
            CborReaderState.Tag => ReadTaggedValue(reader),
            _ => throw new InvalidOperationException($"Unexpected CBOR reader state '{state}' during DOM parsing."),
        };
    }

    private static Node ReadNull(CborReader reader)
    {
        reader.ReadNull();
        return Node.Null;
    }

    private static Node ReadMap(CborReader reader)
    {
        var count = reader.ReadStartMap() ?? throw new InvalidOperationException("Indefinite-length maps are not supported.");

        var properties = new List<NodeProperty>((int)count);
        for (var i = 0; i < (int)count; i++)
        {
            var key = reader.ReadTextString();
            var value = ReadNode(reader);
            properties.Add(new NodeProperty(key, value));
        }

        reader.ReadEndMap();
        return Node.CreateObject(properties);
    }

    private static Node ReadArray(CborReader reader)
    {
        var count = reader.ReadStartArray() ?? throw new InvalidOperationException("Indefinite-length arrays are not supported.");

        var items = new List<Node>((int)count);
        for (var i = 0; i < (int)count; i++)
        {
            items.Add(ReadNode(reader));
        }

        reader.ReadEndArray();
        return Node.CreateArray(items);
    }

    private static Node ReadTaggedValue(CborReader reader)
    {
        var tag = reader.ReadTag();

        return tag switch
        {
            _ when tag == GuidCborConverter.UuidTag => ReadGuidTag(reader),
            _ when tag == DateTimeCborConverter.EpochTag => ReadDateTimeTag(reader),
            _ => ReadNode(reader), // Unknown tag — consume the tag and parse the data item normally
        };
    }

    private static Node ReadGuidTag(CborReader reader)
    {
        var bytes = reader.ReadByteString();
        var guid = new Guid(bytes, bigEndian: true);
        return Node.CreateString(guid.ToString());
    }

    private static Node ReadDateTimeTag(CborReader reader)
    {
        var epochMs = reader.ReadInt64();
        var dateTime = UnixEpoch.AddMilliseconds(epochMs);
        return Node.CreateString(dateTime.ToString("O"));
    }
}
