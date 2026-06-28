using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace D20Tek.Serialization.Generation;

/// <summary>
/// Incremental source generator that emits AOT-friendly serializers and a per-assembly
/// registry for types annotated with <c>[Serializable]</c>.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class SerializableGenerator : IIncrementalGenerator
{
    private const string SerializableAttributeName = "D20Tek.Serialization.SerializableAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var models = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                SerializableAttributeName,
                predicate: static (node, _) => node is Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax,
                transform: static (ctx, _) => ModelBuilder.Build((INamedTypeSymbol)ctx.TargetSymbol))
            .WithComparer(EqualityComparer<SerializableTypeModel>.Default);

        context.RegisterSourceOutput(models, static (spc, model) => EmitSerializer(spc, model));

        var collected = models.Collect();
        context.RegisterSourceOutput(collected, static (spc, all) => EmitRegistry(spc, all));
    }

    private static void EmitSerializer(SourceProductionContext context, SerializableTypeModel model)
    {
        foreach (var diagnostic in model.Diagnostics)
        {
            context.ReportDiagnostic(diagnostic.ToDiagnostic());
        }

        var source = SerializerEmitter.Emit(model);
        context.AddSource($"{model.FlattenedIdentifier}.Serializer.g.cs", source);
    }

    private static void EmitRegistry(SourceProductionContext context, ImmutableArray<SerializableTypeModel> models)
    {
        if (models.IsDefaultOrEmpty) return;

        var source = RegistryEmitter.Emit(models);
        context.AddSource("GeneratedSerializerRegistry.g.cs", source);
    }
}

