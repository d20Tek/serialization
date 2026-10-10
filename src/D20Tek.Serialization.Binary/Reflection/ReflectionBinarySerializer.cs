using D20Tek.Serialization.Metadata;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Binary.Reflection;

/// <summary>
/// The reflection-based serialization fallback that uses <see cref="TypeMetadata"/> to
/// write and read objects through <see cref="IFormatWriter"/>/<see cref="IFormatReader"/>.
/// This is format-agnostic: it drives the abstract interfaces and does not reference CBOR
/// encoding details directly.
/// </summary>
/// <remarks>
/// This path is not AOT/trim-safe. It requires unreferenced code and dynamic code because it
/// uses reflection (via <see cref="TypeMetadataBuilder"/>) to discover and access members at
/// runtime.
/// </remarks>
internal sealed partial class ReflectionBinarySerializer
{
    private const string ReflectionMessage =
        "The reflection-based serialization fallback dynamically reflects over and accesses " +
        "type members and is not compatible with trimming or ahead-of-time compilation.";

    private readonly TypeMetadataProvider _provider = new();

    /// <summary>
    /// Serializes a value of type <typeparamref name="T"/> to the specified writer.
    /// </summary>
    [RequiresUnreferencedCode(ReflectionMessage)]
    [RequiresDynamicCode(ReflectionMessage)]
    public void Write<T>(IFormatWriter writer, T value, BinarySerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(options);

        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        WriteObject(writer, value, typeof(T), options);
    }

    /// <summary>
    /// Deserializes a value of type <typeparamref name="T"/> from the specified reader.
    /// </summary>
    [RequiresUnreferencedCode(ReflectionMessage)]
    [RequiresDynamicCode(ReflectionMessage)]
    public T? Read<T>(IFormatReader reader, BinarySerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(options);

        if (reader.IsNull()) return default;

        return (T?)ReadObject(reader, typeof(T), options, "$");
    }
}
