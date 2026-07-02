using D20Tek.Serialization.Binary.Cbor;
using D20Tek.Serialization.Generation;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization;

/// <summary>
/// Provides static methods for serializing and deserializing values to and from the binary
/// (CBOR) format. This is the primary public entry point for the
/// <c>D20Tek.Serialization.Binary</c> package.
/// </summary>
/// <remarks>
/// Each operation follows a three-tier resolution order:
/// <list type="number">
///   <item><description>
///     <b>Custom converter</b> — a <see cref="Converter"/> registered in
///     <see cref="SerializerOptions.Converters"/> whose
///     <see cref="Converter.CanConvert(Type)"/> returns <see langword="true"/> for the target
///     type.
///   </description></item>
///   <item><description>
///     <b>Generated serializer</b> — an <see cref="IGeneratedSerializer{T}"/> discovered via
///     the <see cref="GeneratedSerializerRegistryAttribute"/> on the assembly that defines the
///     target type.
///   </description></item>
///   <item><description>
///     <b>Reflection fallback</b> — reflection-based serializer (non-AOT only).
///   </description></item>
/// </list>
/// When <c>options</c> is <see langword="null"/>, a default
/// <see cref="BinarySerializerOptions"/> instance is created automatically.
/// </remarks>
public static class BinarySerializer
{
    /// <summary>
    /// Deserializes a value of type <typeparamref name="T"/> from a CBOR-encoded byte span.
    /// </summary>
    /// <typeparam name="T">The type to deserialize.</typeparam>
    /// <param name="data">The CBOR-encoded source bytes.</param>
    /// <param name="options">
    /// Optional serializer options. When <see langword="null"/>, defaults are used.
    /// </param>
    /// <returns>The deserialized value, or <see langword="null"/> for a CBOR null.</returns>
    [RequiresUnreferencedCode(SerializerResolutionHelper.ReflectionFallbackMessage)]
    [RequiresDynamicCode(SerializerResolutionHelper.ReflectionFallbackMessage)]
    public static T? Deserialize<T>(ReadOnlySpan<byte> data, BinarySerializerOptions? options = null)
    {
        var resolved = BinaryOptionsResolver.Resolve(options);
        var reader = new CborFormatReader(data.ToArray(), resolved.DecodingMode);

        // 1. Custom converter
        if (SerializerResolutionHelper.TryGetConverter<T>(resolved, out var converter))
        {
            return converter.Read(reader, resolved);
        }

        // 2. Generated serializer
        if (SerializerResolutionHelper.TryGetGeneratedSerializer<T>(out var generated))
        {
            return generated.Read(reader, resolved);
        }

        // 3. Reflection fallback
        return SerializerResolutionHelper.GetReflectionSerializer().Read<T>(reader, resolved);
    }

    /// <summary>
    /// Deserializes a value of the specified <paramref name="returnType"/> from a CBOR-encoded
    /// byte span.
    /// </summary>
    /// <param name="data">The CBOR-encoded source bytes.</param>
    /// <param name="returnType">The type to deserialize.</param>
    /// <param name="options">
    /// Optional serializer options. When <see langword="null"/>, defaults are used.
    /// </param>
    /// <returns>The deserialized value, or <see langword="null"/> for a CBOR null.</returns>
    /// <remarks>
    /// This overload cannot leverage the generated-serializer tier because the registry
    /// contract is generic (<see cref="IGeneratedSerializerRegistry.TryGetSerializer{T}"/>).
    /// Resolution is: custom converter → reflection fallback.
    /// </remarks>
    [RequiresUnreferencedCode(SerializerResolutionHelper.ReflectionFallbackMessage)]
    [RequiresDynamicCode(SerializerResolutionHelper.ReflectionFallbackMessage)]
    public static object? Deserialize(ReadOnlySpan<byte> data, Type returnType, BinarySerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(returnType);

        var resolved = BinaryOptionsResolver.Resolve(options);
        var reader = new CborFormatReader(data.ToArray(), resolved.DecodingMode);

        // 1. Custom converter — use the non-generic Read via Converter<T>.Read cast
        foreach (var candidate in resolved.Converters)
        {
            if (candidate.CanConvert(returnType))
            {
                return SerializerResolutionHelper.ReadViaConverter(candidate, reader, resolved);
            }
        }

        // 2. Reflection fallback (generated serializer skipped — see remarks)
        return SerializerResolutionHelper.DeserializeViaReflection(reader, returnType, resolved);
    }

    /// <summary>
    /// Serializes a value of type <typeparamref name="T"/> to a new byte array.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <param name="options">
    /// Optional serializer options. When <see langword="null"/>, defaults are used.
    /// </param>
    /// <returns>A byte array containing the CBOR-encoded value.</returns>
    [RequiresUnreferencedCode(SerializerResolutionHelper.ReflectionFallbackMessage)]
    [RequiresDynamicCode(SerializerResolutionHelper.ReflectionFallbackMessage)]
    public static byte[] SerializeToByteArray<T>(T value, BinarySerializerOptions? options = null)
    {
        var resolved = BinaryOptionsResolver.Resolve(options);
        var writer = new CborFormatWriter();

        SerializerResolutionHelper.WriteValue(writer, value, resolved);
        return writer.Encode();
    }

    /// <summary>
    /// Serializes a value of type <typeparamref name="T"/> to the supplied buffer writer.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="bufferWriter">The buffer writer that receives the CBOR-encoded output.</param>
    /// <param name="value">The value to serialize.</param>
    /// <param name="options">
    /// Optional serializer options. When <see langword="null"/>, defaults are used.
    /// </param>
    [RequiresUnreferencedCode(SerializerResolutionHelper.ReflectionFallbackMessage)]
    [RequiresDynamicCode(SerializerResolutionHelper.ReflectionFallbackMessage)]
    public static void Serialize<T>(
        IBufferWriter<byte> bufferWriter,
        T value,
        BinarySerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(bufferWriter);

        var resolved = BinaryOptionsResolver.Resolve(options);
        var writer = new CborFormatWriter();

        SerializerResolutionHelper.WriteValue(writer, value, resolved);
        writer.Encode(bufferWriter);
    }
}
