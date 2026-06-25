namespace D20Tek.Serialization;

/// <summary>
/// Base type for format-agnostic converters that customize how specific types are
/// serialized and deserialized.
/// </summary>
public abstract class Converter
{
    /// <summary>
    /// Determines whether this converter can handle the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to check for conversion support.</param>
    /// <returns>
    /// <see langword="true"/> if this converter can convert <paramref name="typeToConvert"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public abstract bool CanConvert(Type typeToConvert);
}
