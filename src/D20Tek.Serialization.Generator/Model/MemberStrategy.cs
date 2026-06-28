namespace D20Tek.Serialization.Generation;

/// <summary>
/// Identifies how the generated serializer reads and writes a member, mapping the member's type
/// to the appropriate <c>IFormatWriter</c>/<c>IFormatReader</c> calls.
/// </summary>
internal enum MemberStrategy
{
    /// <summary>A <see cref="bool"/> value written via <c>WriteBoolean</c>/<c>GetBoolean</c>.</summary>
    Boolean,

    /// <summary>A signed/unsigned integral value written as a 64-bit integer.</summary>
    Int64,

    /// <summary>A floating-point value written as a 64-bit double.</summary>
    Double,

    /// <summary>A <see cref="string"/> value.</summary>
    String,

    /// <summary>An enum value written as its underlying 64-bit integer.</summary>
    Enum,
}
