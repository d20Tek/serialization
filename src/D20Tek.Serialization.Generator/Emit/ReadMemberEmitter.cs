using static D20Tek.Serialization.Generation.MemberExpressionHelper;

namespace D20Tek.Serialization.Generation;

/// <summary>
/// Emits the <c>Read</c> method body of a generated <c>IGeneratedSerializer&lt;T&gt;</c>
/// implementation for a single <see cref="SerializableTypeModel"/>.
/// </summary>
internal static class ReadMemberEmitter
{
    public static void EmitRead(SourceWriter writer, SerializableTypeModel model, string readReturnType)
    {
        writer.Line(
            $"public {readReturnType} Read(global::D20Tek.Serialization.IFormatReader reader, " +
            $"global::D20Tek.Serialization.SerializerOptions options)");
        writer.OpenBrace();

        if (!model.IsValueType)
        {
            writer.Line("if (reader.IsNull())");
            writer.OpenBrace();
            writer.Line("return null;");
            writer.CloseBrace();
            writer.Line();
        }

        var useLocals = model.Members.Any(m => m.IsInitOnly || m.ConstructorParameterName is not null);

        if (useLocals)
        {
            foreach (var member in model.Members)
            {
                writer.Line($"{member.FullyQualifiedTypeName} {LocalName(member)} = default!;");
            }
        }
        else
        {
            writer.Line($"var result = new {model.FullyQualifiedName}();");
        }

        foreach (var member in model.Members)
        {
            if (member.IsRequired)
            {
                writer.Line($"var __seen_{member.MemberName} = false;");
            }
        }

        writer.Line("reader.ReadStartObject();");
        writer.Line("while (reader.TryReadPropertyName(out var __name))");
        writer.OpenBrace();

        if (model.Members.Count == 0)
        {
            writer.Line("reader.SkipValue();");
        }
        else
        {
            var first = true;
            foreach (var member in model.Members)
            {
                var keyword = first ? "if" : "else if";
                first = false;
                writer.Line($"{keyword} ({ReadNameConditionExpression(member)})");
                writer.OpenBrace();
                EmitReadMember(writer, member, useLocals);
                if (member.IsRequired)
                {
                    writer.Line($"__seen_{member.MemberName} = true;");
                }

                writer.CloseBrace();
            }

            writer.Line("else");
            writer.OpenBrace();
            writer.Line("reader.SkipValue();");
            writer.CloseBrace();
        }

        writer.CloseBrace();
        writer.Line("reader.ReadEndObject();");
        writer.Line();

        foreach (var member in model.Members)
        {
            if (member.IsRequired)
            {
                writer.Line($"if (!__seen_{member.MemberName})");
                writer.OpenBrace();
                writer.Line(
                    $"throw new global::D20Tek.Serialization.SerializationException(" +
                    $"\"Missing required property '{member.MemberName}'.\", \"$.{member.MemberName}\");");
                writer.CloseBrace();
            }
        }

        if (useLocals)
        {
            var constructorMembers = model.Members.Where(m => m.ConstructorParameterName is not null).ToList();
            var initializerMembers = model.Members.Where(m => m.ConstructorParameterName is null).ToList();

            if (constructorMembers.Count > 0)
            {
                var arguments = string.Join(
                    ", ",
                    constructorMembers.Select(m => $"{m.ConstructorParameterName}: {LocalName(m)}"));

                if (initializerMembers.Count > 0)
                {
                    writer.Line($"var result = new {model.FullyQualifiedName}({arguments})");
                    writer.OpenBrace();
                    foreach (var member in initializerMembers)
                    {
                        writer.Line($"{member.MemberName} = {LocalName(member)},");
                    }

                    writer.CloseBrace(";");
                }
                else
                {
                    writer.Line($"var result = new {model.FullyQualifiedName}({arguments});");
                }
            }
            else
            {
                writer.Line($"var result = new {model.FullyQualifiedName}");
                writer.OpenBrace();
                foreach (var member in model.Members)
                {
                    writer.Line($"{member.MemberName} = {LocalName(member)},");
                }

                writer.CloseBrace(";");
            }
        }

        writer.Line("return result;");
        writer.CloseBrace();
    }

    private static string LocalName(MemberModel member) => $"__value_{member.MemberName}";

    private static void EmitReadMember(SourceWriter writer, MemberModel member, bool useObjectInitializer)
    {
        var target = useObjectInitializer ? LocalName(member) : $"result.{member.MemberName}";

        if (member.IsNullable)
        {
            writer.Line("if (reader.IsNull())");
            writer.OpenBrace();
            writer.Line($"{target} = null;");
            writer.CloseBrace();
            writer.Line("else");
            writer.OpenBrace();
            writer.Line($"{target} = {ReadValueExpression(member)};");
            writer.CloseBrace();
        }
        else
        {
            writer.Line($"{target} = {ReadValueExpression(member)};");
        }
    }

    private static string ReadValueExpression(MemberModel member) =>
        member.Strategy switch
        {
            MemberStrategy.Boolean => "reader.GetBoolean()",
            MemberStrategy.Int64 => $"({UnderlyingNonNullType(member)})reader.GetInt64()",
            MemberStrategy.Double => $"({UnderlyingNonNullType(member)})reader.GetDouble()",
            MemberStrategy.String => "reader.GetString()",
            MemberStrategy.Enum => $"({UnderlyingNonNullType(member)})reader.GetInt64()",
            _ => "default",
        };

    private static string ReadNameConditionExpression(MemberModel member)
    {
        var condition = $"__name == ({SerializedNameExpression(member)})";

        foreach (var alias in member.Aliases)
        {
            condition += $" || __name == {Literal(alias)}";
        }

        return condition;
    }
}
