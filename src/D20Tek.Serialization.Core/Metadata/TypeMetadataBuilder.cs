using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace D20Tek.Serialization.Metadata;

/// <summary>
/// Builds <see cref="TypeMetadata"/> for a type by reflecting over its members, applying the
/// serialization attributes and naming policy, and compiling accessor delegates via expression
/// trees. This path is the non-AOT reflection fallback used when no converter or generated
/// serializer is available.
/// </summary>
internal static class TypeMetadataBuilder
{
    private const string _instanceParameterName = "instance";
    private const string _valueParameterName = "value";
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

        var hasParameterlessConstructor =
            type.IsValueType || type.GetConstructor(Type.EmptyTypes) is not null;
        var constructor = hasParameterlessConstructor ? null : ResolveConstructor(type);
        var constructorParameterIndexes = constructor?.GetParameters()
            .Select((p, index) => (p.Name, index))
            .Where(p => p.Name is not null)
            .ToDictionary(p => p.Name!, p => p.index, StringComparer.OrdinalIgnoreCase);

        foreach (var property in type.GetProperties(MemberFlags))
        {
            if (property.GetIndexParameters().Length > 0 || !property.CanRead || IsIgnored(property)) continue;

            var constructorParameterIndex = constructorParameterIndexes is not null &&
                constructorParameterIndexes.TryGetValue(property.Name, out var index)
                ? index
                : (int?)null;

            if (!property.CanWrite && constructorParameterIndex is null) continue;

            members.Add(CreateMember(
                property, property.PropertyType, namingPolicy, ignoreNull, constructorParameterIndex));
        }

        if (includeFields)
        {
            foreach (var field in type.GetFields(MemberFlags))
            {
                if (IsNotSerializableField(field)) continue;

                members.Add(CreateMember(field, field.FieldType, namingPolicy, ignoreNull, null));
            }
        }

        return new(type, members, constructor);
    }

    /// <summary>
    /// Finds a public constructor whose parameters can be matched (by name, case-insensitively,
    /// and by assignable type) to the type's readable public properties, so an instance can be
    /// constructed directly when no parameterless constructor is available (e.g. positional
    /// records). Prefers the constructor with the most parameters among those that fully match.
    /// </summary>
    [RequiresUnreferencedCode(ReflectionMessage)]
    [RequiresDynamicCode(ReflectionMessage)]
    private static System.Reflection.ConstructorInfo? ResolveConstructor(Type type)
    {
        var properties = type.GetProperties(MemberFlags)
            .Where(p => p.GetIndexParameters().Length == 0 && p.CanRead)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        return type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .Where(c => c.GetParameters().Length > 0)
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault(c => c.GetParameters().All(p =>
                p.Name is not null &&
                properties.TryGetValue(p.Name, out var property) &&
                property.PropertyType == p.ParameterType));
    }

    private static bool IsNotSerializableField(FieldInfo field) => field.IsInitOnly || IsIgnored(field);

    private static bool IsIgnored(MemberInfo member) => member.IsDefined(typeof(IgnoreSerializedAttribute), inherit: true);

    [RequiresUnreferencedCode(ReflectionMessage)]
    [RequiresDynamicCode(ReflectionMessage)]
    private static MemberMetadata CreateMember(
        MemberInfo member,
        Type memberType,
        NamingPolicy? namingPolicy,
        bool ignoreNull,
        int? constructorParameterIndex)
    {
        var nameAttribute = member.GetCustomAttribute<NameSerializedAttribute>(inherit: true);
        var serializedName = ResolveSerializedName(member, nameAttribute, namingPolicy);
        var isRequired = member.IsDefined(typeof(RequiredSerializedAttribute), inherit: true);
        var setter = member is PropertyInfo { CanWrite: false } ? null : CompileSetter(member, memberType);

        return new MemberMetadata(
            member.Name,
            serializedName,
            memberType,
            isRequired,
            ignoreNull,
            CompileGetter(member),
            setter,
            nameAttribute?.Aliases,
            constructorParameterIndex);
    }

    private static string ResolveSerializedName(
        MemberInfo member, NameSerializedAttribute? nameAttribute, NamingPolicy? namingPolicy) =>
        nameAttribute?.Name ?? namingPolicy?.ConvertName(member.Name) ?? member.Name;

    [RequiresDynamicCode(ReflectionMessage)]
    private static Func<object, object?> CompileGetter(MemberInfo member)
    {
        var instance = Expression.Parameter(typeof(object), _instanceParameterName);
        var typedInstance = TypedInstanceExpression(instance, member.DeclaringType!);
        var access = Expression.MakeMemberAccess(typedInstance, member);
        var boxed = Expression.Convert(access, typeof(object));

        return Expression.Lambda<Func<object, object?>>(boxed, instance).Compile();
    }

    [RequiresDynamicCode(ReflectionMessage)]
    private static Action<object, object?> CompileSetter(MemberInfo member, Type memberType)
    {
        var instance = Expression.Parameter(typeof(object), _instanceParameterName);
        var value = Expression.Parameter(typeof(object), _valueParameterName);
        var typedInstance = TypedInstanceExpression(instance, member.DeclaringType!);
        var typedValue = Expression.Convert(value, memberType);
        var access = Expression.MakeMemberAccess(typedInstance, member);
        var assign = Expression.Assign(access, typedValue);

        return Expression.Lambda<Action<object, object?>>(assign, instance, value).Compile();
    }

    private static UnaryExpression TypedInstanceExpression(ParameterExpression instance, Type declaringType) =>
        declaringType.IsValueType
            ? Expression.Unbox(instance, declaringType)
            : Expression.Convert(instance, declaringType);
}
