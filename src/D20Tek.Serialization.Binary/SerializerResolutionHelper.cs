using D20Tek.Serialization.Binary.Cbor;
using D20Tek.Serialization.Binary.Reflection;
using D20Tek.Serialization.Generation;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace D20Tek.Serialization;

/// <summary>
/// Encapsulates the three-tier serializer resolution logic used by <see cref="BinarySerializer"/>:
/// <list type="number">
///   <item><description>Custom converter</description></item>
///   <item><description>Generated serializer</description></item>
///   <item><description>Reflection fallback</description></item>
/// </list>
/// </summary>
internal static class SerializerResolutionHelper
{
    internal const string ReflectionFallbackMessage =
        "The BinarySerializer may fall back to the reflection-based serializer, which " +
        "dynamically reflects over type members and is not compatible with trimming or " +
        "ahead-of-time compilation. Use the [Serializable] attribute with source generation " +
        "for an AOT-safe path.";

    private static readonly ReflectionBinarySerializer s_reflectionSerializer = new();
    private static readonly ConditionalWeakTable<ConverterFactory, ConcurrentDictionary<Type, Converter>> conditionalWeakTable = [];

    /// <summary>
    /// Caches converters created by <see cref="ConverterFactory.CreateConverter(Type)"/>,
    /// keyed per-factory-instance then per-type, so repeated resolution for the same closed
    /// generic type does not re-invoke the factory on every call.
    /// </summary>
    private static readonly ConditionalWeakTable<ConverterFactory, ConcurrentDictionary<Type, Converter>>
        s_factoryConverterCache = conditionalWeakTable;

    [RequiresUnreferencedCode(ReflectionFallbackMessage)]
    [RequiresDynamicCode(ReflectionFallbackMessage)]
    internal static void WriteValue<T>(CborFormatWriter writer, T value, BinarySerializerOptions options)
    {
        // 1. Custom converter
        if (TryGetConverter<T>(options, out var converter))
        {
            converter.Write(writer, value!, options);
            return;
        }

        // 2. Generated serializer
        if (TryGetGeneratedSerializer<T>(out var generated))
        {
            generated.Write(writer, value!, options);
            return;
        }

        // 3. Reflection fallback
        GetReflectionSerializer().Write(writer, value, options);
    }

    internal static bool TryGetConverter<T>(
        BinarySerializerOptions options, [MaybeNullWhen(false)] out Converter<T> converter)
    {
        foreach (var candidate in options.Converters)
        {
            if (!candidate.CanConvert(typeof(T)))
            {
                continue;
            }

            if (candidate is Converter<T> typed)
            {
                converter = typed;
                return true;
            }

            if (candidate is ConverterFactory factory &&
                GetOrCreateConverter(factory, typeof(T)) is Converter<T> created)
            {
                converter = created;
                return true;
            }
        }

        converter = null;
        return false;
    }

    /// <summary>
    /// Resolves the non-generic <see cref="Converter"/> instance that will actually handle
    /// <paramref name="typeToConvert"/>: the candidate itself for ordinary converters, or the
    /// converter produced (and cached) by a <see cref="ConverterFactory"/>.
    /// </summary>
    internal static Converter ResolveConverter(Converter candidate, Type typeToConvert) =>
        candidate is ConverterFactory factory ? GetOrCreateConverter(factory, typeToConvert) : candidate;

    private static Converter GetOrCreateConverter(ConverterFactory factory, Type typeToConvert)
    {
        var cache = s_factoryConverterCache.GetOrCreateValue(factory);
        return cache.GetOrAdd(typeToConvert, factory.CreateConverter);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "The generated-serializer path does not use reflection; " +
        "GeneratedSerializerRegistryAttribute.RegistryType preserves the parameterless constructor.")]
    [UnconditionalSuppressMessage("AOT", "IL3050",
        Justification = "Activator.CreateInstance targets a concrete registry type whose " +
        "parameterless constructor is preserved by DynamicallyAccessedMembers on the attribute.")]
    internal static bool TryGetGeneratedSerializer<T>([MaybeNullWhen(false)] out IGeneratedSerializer<T> serializer)
    {
        var attr = typeof(T).Assembly.GetCustomAttribute<GeneratedSerializerRegistryAttribute>();
        if (attr is not null)
        {
            var registry = (IGeneratedSerializerRegistry)Activator.CreateInstance(attr.RegistryType)!;
            if (registry.TryGetSerializer(out serializer))
            {
                return true;
            }
        }

        serializer = null;
        return false;
    }

    [RequiresUnreferencedCode(ReflectionFallbackMessage)]
    [RequiresDynamicCode(ReflectionFallbackMessage)]
    internal static ReflectionBinarySerializer GetReflectionSerializer() => s_reflectionSerializer;

    /// <summary>
    /// Invokes the Read method on a converter found via the non-generic path. The converter
    /// must be a <see cref="Converter{T}"/> — we call Read through its abstract interface.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "The non-generic Deserialize path is gated by RequiresUnreferencedCode.")]
    internal static object? ReadViaConverter(Converter converter, CborFormatReader reader, BinarySerializerOptions options)
    {
        // Converters always derive from Converter<T>. We invoke Read through reflection
        // because we don't have the type parameter at compile time.
        var readMethod = converter.GetType().GetMethod(
            nameof(Converter<>.Read),
            [typeof(IFormatReader), typeof(SerializerOptions)]);
        return readMethod!.Invoke(converter, [reader, options]);
    }

    [RequiresUnreferencedCode(ReflectionFallbackMessage)]
    [RequiresDynamicCode(ReflectionFallbackMessage)]
    [UnconditionalSuppressMessage("Trimming", "IL2060", Justification = "MakeGenericMethod is guarded by RequiresUnreferencedCode on this method.")]
    internal static object? DeserializeViaReflection(
        CborFormatReader reader, Type returnType,BinarySerializerOptions options)
    {
        var serializer = GetReflectionSerializer();

        // Call Read<T> via reflection with the runtime type
        var readMethod = typeof(ReflectionBinarySerializer)
            .GetMethod(nameof(ReflectionBinarySerializer.Read))!
            .MakeGenericMethod(returnType);

        return readMethod.Invoke(serializer, [reader, options]);
    }
}
