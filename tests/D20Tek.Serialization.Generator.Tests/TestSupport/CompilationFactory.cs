using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Generator.Tests.TestSupport;

/// <summary>
/// Builds in-memory C# compilations for exercising the source generator and its helpers, wiring
/// up the runtime reference set plus the <c>D20Tek.Serialization.Core</c> assembly that the
/// generated and model-built code depends on.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class CompilationFactory
{
    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Latest);

    private static readonly CSharpCompilationOptions CompilationOptions =
        new(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable);

    public static CSharpCompilation Create(string source, string assemblyName = "GeneratorTests.Subject") =>
        CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source, ParseOptions)],
            BuildReferences(),
            CompilationOptions);

    public static INamedTypeSymbol GetTypeSymbol(string source, string metadataName)
    {
        var compilation = Create(source);
        var errors = compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.IsEmpty(
            errors,
            $"Subject source failed to compile: {string.Join("; ", errors.Select(e => e.GetMessage()))}");

        var symbol = compilation.GetTypeByMetadataName(metadataName);
        Assert.IsNotNull(symbol, $"Type '{metadataName}' was not found in the compiled source.");
        return symbol;
    }

    private static List<MetadataReference> BuildReferences()
    {
        var references = new List<MetadataReference>();

        var trustedAssemblies =
            (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string)?.Split(Path.PathSeparator) ?? [];
        foreach (var path in trustedAssemblies)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                references.Add(MetadataReference.CreateFromFile(path));
            }
        }

        references.Add(MetadataReference.CreateFromFile(typeof(SerializableAttribute).Assembly.Location));
        return references;
    }
}
