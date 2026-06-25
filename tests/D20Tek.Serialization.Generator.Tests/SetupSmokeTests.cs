using D20Tek.Serialization.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace D20Tek.Serialization.Generator.Tests;

[TestClass]
public sealed class SetupSmokeTests
{
    [TestMethod]
    public void Generator_RunsWithoutDiagnostics()
    {
        // arrange

        // Validates the generator project is referenced and the Roslyn 4.x incremental
        // driver harness is configured. Snapshot/output assertions arrive in task 2.10.7.
        var compilation = CSharpCompilation.Create(
            assemblyName: "Smoke",
            syntaxTrees: [CSharpSyntaxTree.ParseText("public class Probe { }", cancellationToken: TestContext.CancellationToken)],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        var driver = CSharpGeneratorDriver.Create(new SerializableGenerator());

        // act
        _ = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics, TestContext.CancellationToken);

        // assert
        Assert.IsTrue(diagnostics.IsEmpty);
    }

    public TestContext TestContext { get; set; }
}
