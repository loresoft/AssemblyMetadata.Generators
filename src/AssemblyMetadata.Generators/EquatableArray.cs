using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace AssemblyMetadata.Generators;

[ExcludeFromCodeCoverage]
public readonly struct EquatableArray<T> : IReadOnlyCollection<T>, IEquatable<EquatableArray<T>>
    where T : IEquatable<T>
{
    public static readonly EquatableArray<T> Empty = new(Array.Empty<T>());

    private readonly T[] _array;

    public EquatableArray(T[] array)
    {
        _array = array;
    }

    public EquatableArray(IEnumerable<T>? array)
    {
        array ??= Enumerable.Empty<T>();
        _array = array.ToArray();
    }

    public bool Equals(EquatableArray<T> array)
    {
        return AsSpan().SequenceEqual(array.AsSpan());
    }

    public override bool Equals(object obj)
    {
        return obj is EquatableArray<T> array && Equals(this, array);
    }

    public override int GetHashCode()
    {
        if (_array is null)
            return 0;

        return HashCode.Seed.CombineAll(_array);
    }

    public ReadOnlySpan<T> AsSpan()
    {
        return _array.AsSpan();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        IEnumerable<T> array = _array ?? Array.Empty<T>();
        return array.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        IEnumerable<T> array = _array ?? Array.Empty<T>();
        return array.GetEnumerator();
    }

    public int Count => _array?.Length ?? 0;

    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right)
    {
        return !left.Equals(right);
    }
}
