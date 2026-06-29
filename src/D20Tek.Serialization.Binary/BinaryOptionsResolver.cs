namespace D20Tek.Serialization;

/// <summary>
/// Resolves the effective <see cref="BinarySerializerOptions"/> for a serialize or deserialize
/// operation, supplying defaults when none are provided and applying any configured
/// <see cref="BinaryProfile"/>.
/// </summary>
internal static class BinaryOptionsResolver
{
    /// <summary>
    /// Returns the effective options for an operation. When <paramref name="options"/> is
    /// <see langword="null"/>, a new instance with default settings is created. When a
    /// <see cref="BinarySerializerOptions.Profile"/> is set, its
    /// <see cref="BinaryProfile.Configure(BinarySerializerOptions)"/> method is applied to the
    /// resolved instance.
    /// </summary>
    /// <param name="options">The caller-supplied options, or <see langword="null"/>.</param>
    /// <returns>The resolved options instance with any profile applied.</returns>
    internal static BinarySerializerOptions Resolve(BinarySerializerOptions? options)
    {
        var resolved = options ?? new BinarySerializerOptions();
        resolved.Profile?.Configure(resolved);
        return resolved;
    }
}
