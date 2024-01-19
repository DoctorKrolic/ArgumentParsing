using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ArgumentParsing.Generators.Utils;

internal static class ImmutableEquatableArray
{
    public static ImmutableEquatableArray<T> AsEquatableArray<T>(this ImmutableArray<T> array)
        where T : IEquatable<T>
    {
        return new(array);
    }
}

internal readonly struct ImmutableEquatableArray<T>(ImmutableArray<T> array) : IEquatable<ImmutableEquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    private readonly T[]? _array = Unsafe.As<ImmutableArray<T>, T[]?>(ref array);

    public static readonly ImmutableEquatableArray<T> Empty = new(ImmutableArray<T>.Empty);

    public ref readonly T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref AsImmutableArray().ItemRef(index);
    }

    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => AsImmutableArray().IsEmpty;
    }

    public bool IsDefaultOrEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => AsImmutableArray().IsDefaultOrEmpty;
    }

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => AsImmutableArray().Length;
    }

    public bool Equals(ImmutableEquatableArray<T> array)
    {
        return AsSpan().SequenceEqual(array.AsSpan());
    }

    public override bool Equals(object? obj)
    {
        return obj is ImmutableEquatableArray<T> array && Equals(this, array);
    }

    public override unsafe int GetHashCode()
    {
        if (_array is not T[] array)
        {
            return 0;
        }

        HashCode hashCode = default;

        if (typeof(T) == typeof(byte))
        {
            ReadOnlySpan<T> span = array;
            ref T r0 = ref MemoryMarshal.GetReference(span);
            ref byte r1 = ref Unsafe.As<T, byte>(ref r0);

            fixed (byte* p = &r1)
            {
                ReadOnlySpan<byte> bytes = new(p, span.Length);

                hashCode.AddBytes(bytes);
            }
        }
        else
        {
            foreach (var item in array)
            {
                hashCode.Add(item);
            }
        }

        return hashCode.ToHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableArray<T> AsImmutableArray()
    {
        return Unsafe.As<T[]?, ImmutableArray<T>>(ref Unsafe.AsRef(in _array));
    }

    public static ImmutableEquatableArray<T> FromImmutableArray(ImmutableArray<T> array)
    {
        return new(array);
    }

    public ReadOnlySpan<T> AsSpan()
    {
        return AsImmutableArray().AsSpan();
    }

    public T[] ToArray()
    {
        return [.. AsImmutableArray()];
    }

    public ImmutableArray<T>.Enumerator GetEnumerator()
    {
        return AsImmutableArray().GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return ((IEnumerable<T>)AsImmutableArray()).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)AsImmutableArray()).GetEnumerator();
    }

    public static implicit operator ImmutableEquatableArray<T>(ImmutableArray<T> array)
    {
        return FromImmutableArray(array);
    }

    public static implicit operator ImmutableArray<T>(ImmutableEquatableArray<T> array)
    {
        return array.AsImmutableArray();
    }

    public static bool operator ==(ImmutableEquatableArray<T> left, ImmutableEquatableArray<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ImmutableEquatableArray<T> left, ImmutableEquatableArray<T> right)
    {
        return !left.Equals(right);
    }
}
