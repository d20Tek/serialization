using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Generation;

/// <summary>
/// Provides lookup of generated serializers within an assembly. The source generator emits one
/// registry implementation per assembly that exposes every generated
/// <see cref="IGeneratedSerializer{T}"/> for the assembly's <c>[Serializable]</c> types.
/// </summary>
public interface IGeneratedSerializerRegistry
{
    /// <summary>
    /// Attempts to retrieve the generated serializer for the specified type.
    /// </summary>
    /// <typeparam name="T">The type to retrieve a serializer for.</typeparam>
    /// <param name="serializer">
    /// When this method returns <see langword="true"/>, contains the generated serializer for
    /// <typeparamref name="T"/>; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a serializer for <typeparamref name="T"/> was found; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    bool TryGetSerializer<T>([MaybeNullWhen(false)] out IGeneratedSerializer<T> serializer);
}
