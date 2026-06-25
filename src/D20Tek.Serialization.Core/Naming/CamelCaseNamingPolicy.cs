namespace D20Tek.Serialization;

/// <summary>
/// A <see cref="NamingPolicy"/> that converts member names to camelCase.
/// </summary>
/// <remarks>
/// The first character (and any leading run of uppercase characters that forms an acronym)
/// is lowercased, while the remainder of the name is preserved. For example,
/// <c>Person</c> becomes <c>person</c>, <c>FirstName</c> becomes <c>firstName</c>, and
/// <c>XMLData</c> becomes <c>xmlData</c>.
/// </remarks>
internal sealed class CamelCaseNamingPolicy : NamingPolicy
{
    /// <inheritdoc />
    public override string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0]))
        {
            return name;
        }

        return string.Create(name.Length, name, static (chars, source) =>
        {
            source.AsSpan().CopyTo(chars);
            FixCasing(chars);
        });
    }

    private static void FixCasing(Span<char> chars)
    {
        for (var i = 0; i < chars.Length; i++)
        {
            if (i == 1 && !char.IsUpper(chars[i]))
            {
                break;
            }

            var hasNext = i + 1 < chars.Length;

            // Stop when the next character is already lowercase, leaving the rest intact.
            if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
            {
                break;
            }

            chars[i] = char.ToLowerInvariant(chars[i]);
        }
    }
}
