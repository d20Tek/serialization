using D20Tek.Serialization.Generation;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace D20Tek.Serialization.Generator.Tests;

[TestClass]
public sealed class SetupSmokeTests
{
    [TestMethod]
    public void Generator_RunsWithoutDiagnostics()
    {
        // Validates the generator project is referenced and the Roslyn 4.x incremental
        // driver harness is configured. Snapshot/output assertions arrive in task 2.10.7.
        var compilation = CSharpCompilation.Create(
            assemblyName: "Smoke",
            syntaxTrees: [CSharpSyntaxTree.ParseText("public class Probe { }", cancellationToken: TestContext.CancellationToken)],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);

        var driver = CSharpGeneratorDriver.Create(new SerializableGenerator());

        _ = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics, TestContext.CancellationToken);

        diagnostics.Should().BeEmpty();
    }

    public TestContext TestContext { get; set; }
}
