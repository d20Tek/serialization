using Microsoft.CodeAnalysis;

namespace D20Tek.Serialization.Generation;

/// <summary>
/// Incremental source generator that emits AOT-friendly serializers and a per-assembly
/// registry for types annotated with <c>[Serializable]</c>.
/// </summary>
/// <remarks>
/// This is a scaffolding placeholder established during repository setup. The discovery,
/// model-building, and emit logic are implemented in Phase 1 (tasks 2.10.1 - 2.10.7).
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class SerializableGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Phase 1 (task 2.10) wires up discovery of [Serializable] types,
        // builds the serialization model, and emits serializers + registry here.
    }
}
