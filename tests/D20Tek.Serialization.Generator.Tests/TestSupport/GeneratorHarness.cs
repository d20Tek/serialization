using D20Tek.Serialization.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Generator.Tests.TestSupport;

/// <summary>
/// Runs <see cref="SerializableGenerator"/> over an in-memory compilation and exposes the
/// generated sources, generator diagnostics, and the resulting (post-generation) compilation so
/// tests can both inspect emitted code and verify that it compiles.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class GeneratorRunResult
{
    private readonly Compilation _outputCompilation;

    private GeneratorRunResult(
        Compilation outputCompilation,
        ImmutableArray<Diagnostic> generatorDiagnostics,
        IReadOnlyDictionary<string, string> generatedSources)
    {
        _outputCompilation = outputCompilation;
        GeneratorDiagnostics = generatorDiagnostics;
        GeneratedSources = generatedSources;
    }

    public ImmutableArray<Diagnostic> GeneratorDiagnostics { get; }

    public IReadOnlyDictionary<string, string> GeneratedSources { get; }

    public static GeneratorRunResult Run(string source)
    {
        var compilation = CompilationFactory.Create(source);
        return Run(compilation);
    }

    public static GeneratorRunResult Run(Compilation compilation)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SerializableGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var diagnostics);

        var runResult = driver.GetRunResult();
        var sources = runResult.Results
            .SelectMany(r => r.GeneratedSources)
            .ToDictionary(s => s.HintName, s => s.SourceText.ToString());

        return new(outputCompilation, diagnostics, sources);
    }

    public string GetSourceContaining(string fragment)
    {
        var match = GeneratedSources
            .Where(kvp => kvp.Key.Contains(fragment, StringComparison.Ordinal))
            .Select(kvp => kvp.Value)
            .FirstOrDefault();

        Assert.IsNotNull
            (match,
            $"No generated source with hint name containing '{fragment}'. Hint names: {string.Join(", ", GeneratedSources.Keys)}");

        return match;
    }

    /// <summary>Gets the compile errors in the post-generation compilation.</summary>
    public ImmutableArray<Diagnostic> GetCompilationErrors() =>
        [.. _outputCompilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error)];
}
