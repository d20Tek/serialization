using Microsoft.CodeAnalysis;

namespace D20Tek.Serialization.Generation;

/// <summary>
/// A value-equatable description of a source location, suitable for flowing through the
/// incremental generator pipeline (unlike <see cref="Location"/>, which is not cache-friendly).
/// </summary>
internal sealed record LocationInfo(string FilePath, TextSpanInfo Span, LinePositionSpanInfo LineSpan)
{
    public Location ToLocation() =>
        Location.Create(
            FilePath,
            new Microsoft.CodeAnalysis.Text.TextSpan(Span.Start, Span.Length),
            new Microsoft.CodeAnalysis.Text.LinePositionSpan(
                new Microsoft.CodeAnalysis.Text.LinePosition(LineSpan.StartLine, LineSpan.StartCharacter),
                new Microsoft.CodeAnalysis.Text.LinePosition(LineSpan.EndLine, LineSpan.EndCharacter)));

    public static LocationInfo? CreateFrom(Location location)
    {
        if (location.SourceTree is null)
        {
            return null;
        }

        var span = location.SourceSpan;
        var lineSpan = location.GetLineSpan().Span;
        return new LocationInfo(
            location.SourceTree.FilePath,
            new TextSpanInfo(span.Start, span.Length),
            new LinePositionSpanInfo(
                lineSpan.Start.Line,
                lineSpan.Start.Character,
                lineSpan.End.Line,
                lineSpan.End.Character));
    }
}

/// <summary>A value-equatable mirror of <see cref="Microsoft.CodeAnalysis.Text.TextSpan"/>.</summary>
internal readonly record struct TextSpanInfo(int Start, int Length);

/// <summary>A value-equatable mirror of a line/character span.</summary>
internal readonly record struct LinePositionSpanInfo(int StartLine, int StartCharacter, int EndLine, int EndCharacter);
