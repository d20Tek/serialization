using D20Tek.Serialization.Errors;
using System.Formats.Cbor;

namespace D20Tek.Serialization.Binary.Cbor;

/// <summary>
/// A CBOR implementation of <see cref="IFormatReader"/> built on top of
/// <see cref="CborReader"/>. Only definite-length encodings are accepted; indefinite-length
/// items are rejected as a v1 invariant (spec §3.1).
/// </summary>
/// <remarks>
/// The reader provides zero-copy access to string and number payloads via
/// <see cref="GetRawStringBytes"/> and <see cref="GetRawNumberBytes"/>, which return slices of
/// the original source buffer without allocating managed strings (spec §3.4). A
/// <see cref="SerializationPathBuilder"/> tracks the current object property names and array
/// indices so that a JSONPath-style location (for example <c>$.items[3].price</c>) can be
/// attached to any <see cref="SerializationException"/> raised on a type mismatch (spec §3.5).
/// The configured <see cref="DecodingMode"/> controls how unknown semantic tags are treated
/// when values are skipped: <see cref="BinaryDecodingMode.Strict"/> rejects them while
/// <see cref="BinaryDecodingMode.Lenient"/> skips them.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="CborFormatReader"/> class over the supplied
/// CBOR document.
/// </remarks>
/// <param name="data">
/// The CBOR-encoded source buffer. The buffer must remain alive and unmodified for as long
/// as any span returned by <see cref="GetRawStringBytes"/> or <see cref="GetRawNumberBytes"/>
/// is in use, because those spans reference this buffer directly.
/// </param>
/// <param name="decodingMode">
/// The decoding mode that controls how unknown tags are handled. The default is
/// <see cref="BinaryDecodingMode.Lenient"/>.
/// </param>
internal sealed partial class CborFormatReader(ReadOnlyMemory<byte> data, BinaryDecodingMode decodingMode = BinaryDecodingMode.Lenient)
    : IFormatReader
{
    private readonly CborReader _reader = new(data, CborConformanceMode.Lax, allowMultipleRootLevelValues: false);
    private readonly BinaryDecodingMode _decodingMode = decodingMode;
    private readonly SerializationPathBuilder _path = new();
    private readonly Stack<Scope> _scopes = new();

    /// <summary>
    /// Gets the decoding mode that governs how unknown tags are handled.
    /// </summary>
    public BinaryDecodingMode DecodingMode => _decodingMode;

    /// <inheritdoc />
    public ValueKind ValueKind => CborReadErrors.MapState(_reader.PeekState());
}
