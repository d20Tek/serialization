using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace D20Tek.Serialization.Metadata;

/// <summary>
/// Resolves polymorphic serialization metadata for base types and interfaces annotated with
/// <see cref="PolymorphicAttribute"/>, mapping between registered derived types and their wire
/// discriminator values.
/// </summary>
internal static class PolymorphicTypeResolver
{
    private static readonly ConcurrentDictionary<Type, PolymorphicTypeInfo?> s_cache = new();

    /// <summary>
    /// Attempts to resolve polymorphic metadata for the specified declared type.
    /// </summary>
    /// <param name="declaredType">The declared (base or interface) member type.</param>
    /// <param name="info">
    /// When this method returns <see langword="true"/>, contains the resolved polymorphic
    /// metadata for <paramref name="declaredType"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="declaredType"/> is annotated with
    /// <see cref="PolymorphicAttribute"/>; otherwise <see langword="false"/>.
    /// </returns>
    [RequiresUnreferencedCode(
        "Polymorphic type resolution reflects over custom attributes and is not compatible " +
        "with trimming.")]
    public static bool TryResolve(Type declaredType, [MaybeNullWhen(false)] out PolymorphicTypeInfo info)
    {
        ArgumentNullException.ThrowIfNull(declaredType);

        info = s_cache.GetOrAdd(declaredType, Build);
        return info is not null;
    }

    [RequiresUnreferencedCode(
        "Polymorphic type resolution reflects over custom attributes and is not compatible " +
        "with trimming.")]
    private static PolymorphicTypeInfo? Build(Type declaredType)
    {
        var polymorphic = declaredType.GetCustomAttribute<PolymorphicAttribute>(inherit: false);
        if (polymorphic is null) return null;

        var derivedAttributes = declaredType.GetCustomAttributes<SerializedDerivedTypeAttribute>(inherit: false);

        var byDiscriminator = new Dictionary<string, Type>(StringComparer.Ordinal);
        var byType = new Dictionary<Type, string>();

        foreach (var attr in derivedAttributes)
        {
            byDiscriminator[attr.TypeDiscriminator] = attr.DerivedType;
            byType[attr.DerivedType] = attr.TypeDiscriminator;
        }

        return new PolymorphicTypeInfo(
            polymorphic.TypeDiscriminatorPropertyName,
            byDiscriminator,
            byType);
    }
}

/// <summary>
/// Describes the resolved polymorphic serialization metadata for a base type or interface:
/// the type discriminator property name and the bidirectional mapping between discriminator
/// values and concrete derived types.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PolymorphicTypeInfo"/> class.
/// </remarks>
/// <param name="discriminatorPropertyName">The wire property name for the type discriminator.</param>
/// <param name="typesByDiscriminator">Maps discriminator values to concrete derived types.</param>
/// <param name="discriminatorsByType">Maps concrete derived types to discriminator values.</param>
internal sealed class PolymorphicTypeInfo(
    string discriminatorPropertyName,
    IReadOnlyDictionary<string, Type> typesByDiscriminator,
    IReadOnlyDictionary<Type, string> discriminatorsByType)
{
    /// <summary>
    /// Gets the wire property name used for the type discriminator.
    /// </summary>
    public string DiscriminatorPropertyName { get; } = discriminatorPropertyName;

    /// <summary>
    /// Gets the mapping from discriminator values to concrete derived types.
    /// </summary>
    public IReadOnlyDictionary<string, Type> TypesByDiscriminator { get; } = typesByDiscriminator;

    /// <summary>
    /// Gets the mapping from concrete derived types to discriminator values.
    /// </summary>
    public IReadOnlyDictionary<Type, string> DiscriminatorsByType { get; } = discriminatorsByType;

    /// <summary>
    /// Attempts to resolve the discriminator value registered for the specified runtime type.
    /// </summary>
    /// <param name="runtimeType">The concrete runtime type of the value being serialized.</param>
    /// <param name="discriminator">
    /// When this method returns <see langword="true"/>, contains the discriminator value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="runtimeType"/> was registered; otherwise
    /// <see langword="false"/>.
    /// </returns>
    public bool TryGetDiscriminator(Type runtimeType, [MaybeNullWhen(false)] out string discriminator) =>
        DiscriminatorsByType.TryGetValue(runtimeType, out discriminator);

    /// <summary>
    /// Attempts to resolve the concrete derived type registered for the specified discriminator
    /// value.
    /// </summary>
    /// <param name="discriminator">The discriminator value read from the wire.</param>
    /// <param name="derivedType">
    /// When this method returns <see langword="true"/>, contains the resolved derived type.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="discriminator"/> was registered; otherwise
    /// <see langword="false"/>.
    /// </returns>
    public bool TryGetType(string discriminator, [MaybeNullWhen(false)] out Type derivedType) =>
        TypesByDiscriminator.TryGetValue(discriminator, out derivedType);
}
