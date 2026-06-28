using System.Collections;
using System.Collections.Immutable;

namespace D20Tek.Serialization.Generation;

/// <summary>
/// A small wrapper around <see cref="ImmutableArray{T}"/> that implements value equality over its
/// elements. Incremental source generators require model values to be equatable so the pipeline
/// can cache and short-circuit unchanged work; the default <see cref="ImmutableArray{T}"/> uses
/// reference equality, which defeats caching.
/// </summary>
/// <typeparam name="T">The element type, which must itself be equatable.</typeparam>
internal readonly struct EquatableArray<T>(ImmutableArray<T> array) : IEquatable<EquatableArray<T>>, IReadOnlyList<T>
    where T : IEquatable<T>
{
    public static readonly EquatableArray<T> Empty = new(ImmutableArray<T>.Empty);
    private readonly ImmutableArray<T> _array = array;

    public int Count => _array.IsDefault ? 0 : _array.Length;

    public T this[int index] => _array[index];

    public bool Equals(EquatableArray<T> other)
    {
        if (_array.IsDefault || other._array.IsDefault)
        {
            return _array.IsDefault && other._array.IsDefault;
        }

        if (_array.Length != other._array.Length)
        {
            return false;
        }

        for (var i = 0; i < _array.Length; i++)
        {
            if (!_array[i].Equals(other._array[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        if (_array.IsDefault)
        {
            return 0;
        }

        var hash = 17;
        foreach (var item in _array)
        {
            hash = (hash * 31) + (item?.GetHashCode() ?? 0);
        }

        return hash;
    }

    public IEnumerator<T> GetEnumerator() =>
        (_array.IsDefault ? ImmutableArray<T>.Empty : _array).AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Convenience helpers for constructing <see cref="EquatableArray{T}"/> values.
/// </summary>
internal static class EquatableArray
{
    public static EquatableArray<T> ToEquatableArray<T>(this ImmutableArray<T> array)
        where T : IEquatable<T> => new(array);
}
