using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace D20Tek.Serialization;

/// <summary>
/// Resolves and caches <see cref="TypeMetadata"/> per <c>(Type, options)</c> pair so the
/// reflection serialization fallback reflects over and compiles accessors for each type only
/// once per options instance.
/// </summary>
internal sealed class TypeMetadataProvider
{
    private readonly ConditionalWeakTable<SerializerOptions, ConcurrentDictionary<Type, TypeMetadata>> _cache = [];

    /// <summary>
    /// Gets the metadata for the specified type and options, building and caching it on first use.
    /// </summary>
    /// <param name="type">The type to resolve metadata for.</param>
    /// <param name="options">The options that scope the cache and supply naming/null behavior.</param>
    /// <param name="includeFields">
    /// When <see langword="true"/>, public instance fields are included in addition to properties.
    /// The value is expected to be stable for a given <paramref name="options"/> instance.
    /// </param>
    /// <returns>The cached or newly built metadata for the type.</returns>
    [RequiresUnreferencedCode("The reflection-based serialization fallback is not compatible with trimming.")]
    [RequiresDynamicCode("The reflection-based serialization fallback is not compatible with ahead-of-time compilation.")]
    public TypeMetadata GetOrAdd(Type type, SerializerOptions options, bool includeFields)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(options);

        var perOptions = _cache.GetValue(
            options,
            static _ => new ConcurrentDictionary<Type, TypeMetadata>());

        if (perOptions.TryGetValue(type, out var existing)) return existing;

        var metadata = TypeMetadataBuilder.Build(type, options, includeFields);
        return perOptions.GetOrAdd(type, metadata);
    }
}
