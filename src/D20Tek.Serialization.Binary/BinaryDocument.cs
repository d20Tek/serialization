using D20Tek.Serialization.Binary.Cbor;

namespace D20Tek.Serialization;

/// <summary>
/// Represents a parsed binary (CBOR) document. Call <see cref="Parse(ReadOnlySpan{byte})"/> to
/// materialize a byte payload into a navigable DOM, then use <see cref="RootElement"/> to
/// traverse the tree.
/// </summary>
/// <remarks>
/// <para>
/// This class is the binary‑format equivalent of <c>System.Text.Json.JsonDocument</c>. It owns
/// the materialized <see cref="byte"/> array and the parsed node tree. Disposing the document
/// releases both references so they become eligible for garbage collection.
/// </para>
/// <para>
/// After disposal, accessing <see cref="RootElement"/> throws <see cref="ObjectDisposedException"/>.
/// </para>
/// </remarks>
public sealed class BinaryDocument : IDisposable
{
    private byte[]? _data;
    private BinaryElement _rootElement;
    private bool _disposed;

    private BinaryDocument(byte[] data, BinaryElement rootElement)
    {
        _data = data;
        _rootElement = rootElement;
    }

    /// <summary>
    /// Gets the root element of this binary document.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// The document has been disposed.
    /// </exception>
    public BinaryElement RootElement
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _rootElement;
        }
    }

    /// <summary>
    /// Parses the supplied CBOR-encoded data into a <see cref="BinaryDocument"/>.
    /// </summary>
    /// <param name="data">The CBOR-encoded payload.</param>
    /// <returns>A new <see cref="BinaryDocument"/> containing the parsed tree.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the data contains an unsupported CBOR construct (e.g., indefinite-length containers).
    /// </exception>
    public static BinaryDocument Parse(ReadOnlySpan<byte> data)
    {
        var materialized = data.ToArray();
        var rootNode = CborNodeParser.Parse(materialized);
        return new BinaryDocument(materialized, new BinaryElement(rootNode));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _data = null;
            _rootElement = default;
            _disposed = true;
        }
    }
}
