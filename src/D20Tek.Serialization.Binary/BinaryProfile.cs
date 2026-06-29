namespace D20Tek.Serialization;

/// <summary>
/// Represents a reusable, named bundle of binary serialization settings. Derive from this type
/// to encapsulate a configuration that can be applied to a <see cref="BinarySerializerOptions"/>
/// instance during options resolution.
/// </summary>
public abstract class BinaryProfile
{
    /// <summary>
    /// Gets the unique, human-readable name that identifies this profile.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Applies this profile's settings to the supplied <paramref name="options"/>. The default
    /// implementation makes no changes; override it to mutate the options as needed.
    /// </summary>
    /// <param name="options">The options instance to configure.</param>
    public virtual void Configure(BinarySerializerOptions options)
    {
    }
}
