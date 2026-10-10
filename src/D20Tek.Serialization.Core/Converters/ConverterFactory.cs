namespace D20Tek.Serialization;

/// <summary>
/// Base type for converter factories that produce a <see cref="Converter"/> for a family of
/// related types at runtime, such as open generic types (e.g., <c>Nullable&lt;T&gt;</c> or a
/// custom <c>Dictionary&lt;TKey, TValue&gt;</c> variant). Unlike <see cref="Converter{T}"/>,
/// which handles exactly one closed type, a single <see cref="ConverterFactory"/> can handle an
/// unbounded number of closed generic instantiations without requiring one converter class per
/// combination of type arguments.
/// </summary>
/// <remarks>
/// Register factory instances in <see cref="SerializerOptions.Converters"/> alongside ordinary
/// converters. During resolution, <see cref="Converter.CanConvert(Type)"/> is checked first; if
/// it returns <see langword="true"/>, <see cref="CreateConverter(Type)"/> is invoked to obtain
/// the concrete converter for the requested type, and that converter performs the actual
/// read/write. The converter returned by <see cref="CreateConverter(Type)"/> must itself report
/// <see langword="true"/> from <see cref="Converter.CanConvert(Type)"/> for the same type.
/// </remarks>
public abstract class ConverterFactory : Converter
{
    /// <summary>
    /// Creates a converter capable of reading and writing values of the specified type.
    /// </summary>
    /// <param name="typeToConvert">
    /// The closed type to create a converter for. This is always a type for which
    /// <see cref="Converter.CanConvert(Type)"/> previously returned <see langword="true"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Converter"/> instance (typically a <see cref="Converter{T}"/> closed over
    /// <paramref name="typeToConvert"/>) that performs the actual conversion.
    /// </returns>
    public abstract Converter CreateConverter(Type typeToConvert);
}
