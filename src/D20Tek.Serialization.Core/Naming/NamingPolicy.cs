namespace D20Tek.Serialization;

/// <summary>
/// Defines a strategy for converting .NET member names into their serialized representation.
/// </summary>
public abstract class NamingPolicy
{
    /// <summary>
    /// Converts the specified member name into its serialized form.
    /// </summary>
    /// <param name="name">The .NET member name to convert.</param>
    /// <returns>The converted name to use during serialization.</returns>
    public abstract string ConvertName(string name);

    /// <summary>
    /// Gets the default naming policy, which preserves member names as-is.
    /// A <see langword="null"/> value indicates that no conversion is applied.
    /// </summary>
    public static NamingPolicy? Default => null;

    /// <summary>
    /// Gets a naming policy that converts member names to camelCase.
    /// </summary>
    public static NamingPolicy CamelCase { get; } = new CamelCaseNamingPolicy();
}
