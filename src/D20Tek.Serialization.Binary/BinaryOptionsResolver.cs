using D20Tek.Serialization.Binary.Cbor.Converters;

namespace D20Tek.Serialization;

/// <summary>
/// Resolves the effective <see cref="BinarySerializerOptions"/> for a serialize or deserialize
/// operation, supplying defaults when none are provided and applying any configured
/// <see cref="BinaryProfile"/>. Built-in CBOR converters for <see cref="Guid"/>,
/// <see cref="DateTime"/>, <see cref="DateTimeOffset"/>, and <see cref="decimal"/> are
/// automatically registered unless the user has already provided a converter for those types.
/// </summary>
internal static class BinaryOptionsResolver
{
    /// <summary>
    /// The built-in converters registered by default. <see cref="EnumCborConverter{TEnum}"/> is
    /// generic and cannot be auto-registered; users register it explicitly for each enum type.
    /// </summary>
    private static readonly Converter[] BuiltInConverters =
    [
        new GuidCborConverter(),
        new DateTimeCborConverter(),
        new DateTimeOffsetCborConverter(),
        new DecimalCborConverter(),
    ];

    /// <summary>
    /// Returns the effective options for an operation. When <paramref name="options"/> is
    /// <see langword="null"/>, a new instance with default settings is created. When a
    /// <see cref="BinarySerializerOptions.Profile"/> is set, its
    /// <see cref="BinaryProfile.Configure(BinarySerializerOptions)"/> method is applied to the
    /// resolved instance. Built-in converters are appended for any types not already covered
    /// by user-supplied converters.
    /// </summary>
    /// <param name="options">The caller-supplied options, or <see langword="null"/>.</param>
    /// <returns>The resolved options instance with any profile applied.</returns>
    internal static BinarySerializerOptions Resolve(BinarySerializerOptions? options)
    {
        var resolved = options ?? new BinarySerializerOptions();
        resolved.Profile?.Configure(resolved);
        EnsureBuiltInConverters(resolved);
        return resolved;
    }

    private static void EnsureBuiltInConverters(BinarySerializerOptions options)
    {
        foreach (var builtIn in BuiltInConverters)
        {
            if (!HasUserConverterFor(options, builtIn))
            {
                options.Converters.Add(builtIn);
            }
        }
    }

    internal static bool HasUserConverterFor(BinarySerializerOptions options, Converter builtIn)
    {
        var targetType = GetHandledType(builtIn);
        return options.Converters.Any(existing =>
            ReferenceEquals(existing, builtIn) ||
            (targetType is not null && existing.CanConvert(targetType)));
    }

    internal static Type? GetHandledType(Converter converter) => converter switch
    {
        GuidCborConverter => typeof(Guid),
        DateTimeCborConverter => typeof(DateTime),
        DateTimeOffsetCborConverter => typeof(DateTimeOffset),
        DecimalCborConverter => typeof(decimal),
        _ => null,
    };
}
