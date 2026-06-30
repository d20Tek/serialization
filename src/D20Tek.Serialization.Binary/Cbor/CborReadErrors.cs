using System.Formats.Cbor;

namespace D20Tek.Serialization.Binary.Cbor;

/// <summary>
/// Static factory methods for the <see cref="SerializationException"/> instances thrown by
/// <see cref="CborFormatReader"/>, plus the <see cref="CborReaderState"/>-to-<see cref="ValueKind"/>
/// mapping used for error reporting and the public <see cref="IFormatReader.ValueKind"/> property.
/// Extracting these keeps the reader focused on navigation and value access.
/// </summary>
internal static class CborReadErrors
{
    /// <summary>
    /// Maps a <see cref="CborReaderState"/> to the format-agnostic <see cref="ValueKind"/>.
    /// </summary>
    internal static ValueKind MapState(CborReaderState state) => state switch
    {
        CborReaderState.StartMap => ValueKind.Object,
        CborReaderState.StartArray => ValueKind.Array,
        CborReaderState.TextString or CborReaderState.StartIndefiniteLengthTextString => ValueKind.String,
        CborReaderState.ByteString or CborReaderState.StartIndefiniteLengthByteString => ValueKind.String,
        CborReaderState.UnsignedInteger or CborReaderState.NegativeInteger => ValueKind.Number,
        CborReaderState.HalfPrecisionFloat or CborReaderState.SinglePrecisionFloat
            or CborReaderState.DoublePrecisionFloat => ValueKind.Number,
        CborReaderState.Boolean => ValueKind.Boolean,
        _ => ValueKind.Null,
    };

    /// <summary>
    /// Creates a <see cref="SerializationException"/> for a type mismatch, reporting the
    /// expected kind against the actual kind derived from the current reader state.
    /// </summary>
    /// <param name="expected">The value kind the caller expected.</param>
    /// <param name="readerState">The current <see cref="CborReaderState"/> (used to derive the actual kind).</param>
    /// <param name="path">The JSONPath location of the error.</param>
    internal static SerializationException Mismatch(ValueKind expected, CborReaderState readerState, string path)
    {
        var actual = MapState(readerState);
        return new SerializationException(
            $"Expected {expected} but found {actual}.",
            path,
            expected,
            actual);
    }

    /// <summary>
    /// Creates a <see cref="SerializationException"/> indicating that an indefinite-length item
    /// was encountered, which v1 does not support.
    /// </summary>
    /// <param name="itemKind">A human-readable label for the item kind (e.g. "map", "array", "text string").</param>
    /// <param name="path">The JSONPath location of the error.</param>
    internal static SerializationException IndefiniteNotSupported(string itemKind, string path) =>
        new($"Indefinite-length {itemKind} values are not supported.", path);
}
