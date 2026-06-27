using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace D20Tek.Serialization;

/// <summary>
/// Builds <see cref="TypeMetadata"/> for a type by reflecting over its members, applying the
/// serialization attributes and naming policy, and compiling accessor delegates via expression
/// trees. This path is the non-AOT reflection fallback used when no converter or generated
/// serializer is available.
/// </summary>
internal static class TypeMetadataBuilder
{
    private const string ReflectionMessage =
        "The reflection-based serialization fallback dynamically reflects over and accesses " +
        "type members and is not compatible with trimming or ahead-of-time compilation.";

    private const BindingFlags MemberFlags = BindingFlags.Public | BindingFlags.Instance;

    /// <summary>
    /// Builds metadata for the specified type using the supplied options.
    /// </summary>
    /// <param name="type">The type to reflect over.</param>
    /// <param name="options">The options whose naming policy and null handling are applied.</param>
    /// <param name="includeFields">
    /// When <see langword="true"/>, public instance fields are included in addition to properties.
    /// </param>
    /// <returns>The metadata describing the serializable members of <paramref name="type"/>.</returns>
    /// <remarks>
    /// Only public instance properties that are both readable and writable are included; read-only
    /// or write-only properties and read-only fields are skipped. Members annotated with
    /// <see cref="IgnoreSerializedAttribute"/> are excluded.
    /// </remarks>
    [RequiresUnreferencedCode(ReflectionMessage)]
    [RequiresDynamicCode(ReflectionMessage)]
    public static TypeMetadata Build(Type type, SerializerOptions options, bool includeFields)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(options);

        var namingPolicy = options.PropertyNamingPolicy;
        var ignoreNull = options.IgnoreNullValues;
        var members = new List<MemberMetadata>();

        foreach (var property in type.GetProperties(MemberFlags))
        {
            if (property.GetIndexParameters().Length > 0 || !property.CanRead || !property.CanWrite || IsIgnored(property))
            {
                continue;
            }

            members.Add(CreateMember(property, property.PropertyType, namingPolicy, ignoreNull));
        }

        if (includeFields)
        {
            foreach (var field in type.GetFields(MemberFlags))
            {
                if (field.IsInitOnly || IsIgnored(field))
                {
                    continue;
                }

                members.Add(CreateMember(field, field.FieldType, namingPolicy, ignoreNull));
            }
        }

        return new TypeMetadata(type, members);
    }

    private static bool IsIgnored(MemberInfo member) => member.IsDefined(typeof(IgnoreSerializedAttribute), inherit: true);

    [RequiresUnreferencedCode(ReflectionMessage)]
    [RequiresDynamicCode(ReflectionMessage)]
    private static MemberMetadata CreateMember(
        MemberInfo member,
        Type memberType,
        NamingPolicy? namingPolicy,
        bool ignoreNull)
    {
        var serializedName = ResolveSerializedName(member, namingPolicy);
        var isRequired = member.IsDefined(typeof(RequiredSerializedAttribute), inherit: true);

        return new MemberMetadata(
            member.Name,
            serializedName,
            memberType,
            isRequired,
            ignoreNull,
            CompileGetter(member),
            CompileSetter(member, memberType));
    }

    private static string ResolveSerializedName(MemberInfo member, NamingPolicy? namingPolicy)
    {
        var custom = member.GetCustomAttribute<SerializedNameAttribute>(inherit: true);
        if (custom is not null)
        {
            return custom.Name;
        }

        return namingPolicy?.ConvertName(member.Name) ?? member.Name;
    }

    [RequiresDynamicCode(ReflectionMessage)]
    private static Func<object, object?> CompileGetter(MemberInfo member)
    {
        var instance = Expression.Parameter(typeof(object), "instance");
        var typedInstance = Expression.Convert(instance, member.DeclaringType!);
        var access = Expression.MakeMemberAccess(typedInstance, member);
        var boxed = Expression.Convert(access, typeof(object));

        return Expression.Lambda<Func<object, object?>>(boxed, instance).Compile();
    }

    [RequiresDynamicCode(ReflectionMessage)]
    private static Action<object, object?> CompileSetter(MemberInfo member, Type memberType)
    {
        var instance = Expression.Parameter(typeof(object), "instance");
        var value = Expression.Parameter(typeof(object), "value");
        var typedInstance = Expression.Convert(instance, member.DeclaringType!);
        var typedValue = Expression.Convert(value, memberType);
        var access = Expression.MakeMemberAccess(typedInstance, member);
        var assign = Expression.Assign(access, typedValue);

        return Expression.Lambda<Action<object, object?>>(assign, instance, value).Compile();
    }
}
