using static D20Tek.Serialization.Generation.MemberExpressionHelper;

namespace D20Tek.Serialization.Generation;

/// <summary>
/// Emits the <c>Write</c> method body of a generated <c>IGeneratedSerializer&lt;T&gt;</c>
/// implementation for a single <see cref="SerializableTypeModel"/>.
/// </summary>
internal static class WriteMemberEmitter
{
    public static void EmitWrite(SourceWriter writer, SerializableTypeModel model)
    {
        writer.Line(
            $"public void Write(global::D20Tek.Serialization.IFormatWriter writer, " +
            $"{model.FullyQualifiedName} value, global::D20Tek.Serialization.SerializerOptions options)");
        writer.OpenBrace();

        if (!model.IsValueType)
        {
            writer.Line("if (value is null)");
            writer.OpenBrace();
            writer.Line("writer.WriteNull();");
            writer.Line("return;");
            writer.CloseBrace();
            writer.Line();
        }

        writer.Line("writer.WriteStartObject();");

        foreach (var member in model.Members)
        {
            EmitWriteMember(writer, member);
        }

        writer.Line("writer.WriteEndObject();");
        writer.CloseBrace();
    }

    private static void EmitWriteMember(SourceWriter writer, MemberModel member)
    {
        var access = $"value.{member.MemberName}";
        var nameExpr = SerializedNameExpression(member);

        if (member.IsNullable)
        {
            writer.Line($"if ({access} is null)");
            writer.OpenBrace();
            writer.Line("if (!options.IgnoreNullValues)");
            writer.OpenBrace();
            writer.Line($"writer.WritePropertyName({nameExpr});");
            writer.Line("writer.WriteNull();");
            writer.CloseBrace();
            writer.CloseBrace();
            writer.Line("else");
            writer.OpenBrace();
            writer.Line($"writer.WritePropertyName({nameExpr});");
            EmitWriteValue(writer, member, NonNullAccess(member, access));
            writer.CloseBrace();
        }
        else
        {
            writer.Line($"writer.WritePropertyName({nameExpr});");
            EmitWriteValue(writer, member, access);
        }
    }

    private static void EmitWriteValue(SourceWriter writer, MemberModel member, string valueExpr)
    {
        switch (member.Strategy)
        {
            case MemberStrategy.Boolean:
                writer.Line($"writer.WriteBoolean({valueExpr});");
                break;
            case MemberStrategy.Int64:
                writer.Line($"writer.WriteNumber((long){valueExpr});");
                break;
            case MemberStrategy.Double:
                writer.Line($"writer.WriteNumber((double){valueExpr});");
                break;
            case MemberStrategy.String:
                writer.Line($"writer.WriteString({valueExpr});");
                break;
            case MemberStrategy.Enum:
                writer.Line($"writer.WriteNumber((long){valueExpr});");
                break;
        }
    }
}
