using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization;

public static partial class BinarySerializer
{
    /// <summary>
    /// Serializes a value of type <typeparamref name="T"/> to the supplied <paramref name="stream"/>.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="stream">The stream that receives the CBOR-encoded output.</param>
    /// <param name="value">The value to serialize.</param>
    /// <param name="options">
    /// Optional serializer options. When <see langword="null"/>, defaults are used.
    /// </param>
    /// <remarks>
    /// The value is first encoded into an in-memory buffer, then written to the stream in a
    /// single call. <paramref name="stream"/> is not disposed by this method.
    /// </remarks>
    [RequiresUnreferencedCode(SerializerResolutionHelper.ReflectionFallbackMessage)]
    [RequiresDynamicCode(SerializerResolutionHelper.ReflectionFallbackMessage)]
    public static void Serialize<T>(Stream stream, T value, BinarySerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var resolved = BinaryOptionsResolver.Resolve(options);
        var writer = new Binary.Cbor.CborFormatWriter();

        SerializerResolutionHelper.WriteValue(writer, value, resolved);
        stream.Write(writer.Encode());
    }

    /// <summary>
    /// Deserializes a value of type <typeparamref name="T"/> from a CBOR-encoded
    /// <paramref name="stream"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize.</typeparam>
    /// <param name="stream">The stream containing the CBOR-encoded source bytes.</param>
    /// <param name="options">
    /// Optional serializer options. When <see langword="null"/>, defaults are used.
    /// </param>
    /// <returns>The deserialized value, or <see langword="null"/> for a CBOR null.</returns>
    /// <remarks>
    /// The stream is read to completion into an in-memory buffer before decoding, since CBOR
    /// decoding requires random access to the encoded document.
    /// </remarks>
    [RequiresUnreferencedCode(SerializerResolutionHelper.ReflectionFallbackMessage)]
    [RequiresDynamicCode(SerializerResolutionHelper.ReflectionFallbackMessage)]
    public static T? Deserialize<T>(Stream stream, BinarySerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var buffer = ReadAllBytes(stream);
        return Deserialize<T>(buffer, options);
    }

    /// <summary>
    /// Deserializes a value of the specified <paramref name="returnType"/> from a CBOR-encoded
    /// <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">The stream containing the CBOR-encoded source bytes.</param>
    /// <param name="returnType">The type to deserialize.</param>
    /// <param name="options">
    /// Optional serializer options. When <see langword="null"/>, defaults are used.
    /// </param>
    /// <returns>The deserialized value, or <see langword="null"/> for a CBOR null.</returns>
    /// <remarks>
    /// The stream is read to completion into an in-memory buffer before decoding, since CBOR
    /// decoding requires random access to the encoded document.
    /// </remarks>
    [RequiresUnreferencedCode(SerializerResolutionHelper.ReflectionFallbackMessage)]
    [RequiresDynamicCode(SerializerResolutionHelper.ReflectionFallbackMessage)]
    public static object? Deserialize(Stream stream, Type returnType, BinarySerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(returnType);

        var buffer = ReadAllBytes(stream);
        return Deserialize(buffer, returnType, options);
    }

    private static byte[] ReadAllBytes(Stream stream)
    {
        if (stream is MemoryStream ms && ms.TryGetBuffer(out var segment))
        {
            return segment.AsSpan((int)ms.Position, (int)(ms.Length - ms.Position)).ToArray();
        }

        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }
}
