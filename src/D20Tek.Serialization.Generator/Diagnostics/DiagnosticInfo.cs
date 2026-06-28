using Microsoft.CodeAnalysis;

namespace D20Tek.Serialization.Generation;

/// <summary>
/// A value-equatable carrier for a diagnostic produced while building the serialization model.
/// The pipeline accumulates these and materializes real <see cref="Diagnostic"/> instances only
/// at the reporting stage, keeping the model nodes cache-friendly.
/// </summary>
internal sealed record DiagnosticInfo(
    DiagnosticDescriptor Descriptor,
    LocationInfo? Location,
    EquatableArray<string> MessageArguments)
{
    public Diagnostic ToDiagnostic() =>
        Diagnostic.Create(Descriptor, Location?.ToLocation(), [.. MessageArguments]);
}
