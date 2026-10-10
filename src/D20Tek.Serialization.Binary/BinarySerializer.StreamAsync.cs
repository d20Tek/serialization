using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization;

public static partial class BinarySerializer
{
    /// <summary>
    /// Asynchronously serializes a value of type <typeparamref name="T"/> to the supplied
    /// <paramref name="stream"/>.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="stream">The stream that receives the CBOR-encoded output.</param>
    /// <param name="value">The value to serialize.</param>
    /// <param name="options">
    /// Optional serializer options. When <see langword="null"/>, defaults are used.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the asynchronous write.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    /// <remarks>
    /// The value is first encoded into an in-memory buffer (encoding itself is synchronous),
    /// then written to the stream asynchronously. <paramref name="stream"/> is not disposed by
    /// this method.
    /// </remarks>
    [RequiresUnreferencedCode(SerializerResolutionHelper.ReflectionFallbackMessage)]
    [RequiresDynamicCode(SerializerResolutionHelper.ReflectionFallbackMessage)]
    public static async Task SerializeAsync<T>(
        Stream stream,
        T value,
        BinarySerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var resolved = BinaryOptionsResolver.Resolve(options);
        var writer = new Binary.Cbor.CborFormatWriter();

        SerializerResolutionHelper.WriteValue(writer, value, resolved);
        await stream.WriteAsync(writer.Encode(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously deserializes a value of type <typeparamref name="T"/> from a
    /// CBOR-encoded <paramref name="stream"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize.</typeparam>
    /// <param name="stream">The stream containing the CBOR-encoded source bytes.</param>
    /// <param name="options">
    /// Optional serializer options. When <see langword="null"/>, defaults are used.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the asynchronous read.</param>
    /// <returns>
    /// A task that represents the asynchronous read operation. The task result contains the
    /// deserialized value, or <see langword="null"/> for a CBOR null.
    /// </returns>
    /// <remarks>
    /// The stream is read to completion into an in-memory buffer before decoding (decoding
    /// itself is synchronous), since CBOR decoding requires random access to the encoded
    /// document.
    /// </remarks>
    [RequiresUnreferencedCode(SerializerResolutionHelper.ReflectionFallbackMessage)]
    [RequiresDynamicCode(SerializerResolutionHelper.ReflectionFallbackMessage)]
    public static async Task<T?> DeserializeAsync<T>(
        Stream stream,
        BinarySerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var buffer = await ReadAllBytesAsync(stream, cancellationToken).ConfigureAwait(false);
        return Deserialize<T>(buffer, options);
    }

    /// <summary>
    /// Asynchronously deserializes a value of the specified <paramref name="returnType"/> from
    /// a CBOR-encoded <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">The stream containing the CBOR-encoded source bytes.</param>
    /// <param name="returnType">The type to deserialize.</param>
    /// <param name="options">
    /// Optional serializer options. When <see langword="null"/>, defaults are used.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the asynchronous read.</param>
    /// <returns>
    /// A task that represents the asynchronous read operation. The task result contains the
    /// deserialized value, or <see langword="null"/> for a CBOR null.
    /// </returns>
    /// <remarks>
    /// The stream is read to completion into an in-memory buffer before decoding (decoding
    /// itself is synchronous), since CBOR decoding requires random access to the encoded
    /// document.
    /// </remarks>
    [RequiresUnreferencedCode(SerializerResolutionHelper.ReflectionFallbackMessage)]
    [RequiresDynamicCode(SerializerResolutionHelper.ReflectionFallbackMessage)]
    public static async Task<object?> DeserializeAsync(
        Stream stream,
        Type returnType,
        BinarySerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(returnType);

        var buffer = await ReadAllBytesAsync(stream, cancellationToken).ConfigureAwait(false);
        return Deserialize(buffer, returnType, options);
    }

    private static async Task<byte[]> ReadAllBytesAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
        return buffer.ToArray();
    }
}
