using System.Collections.Generic;

namespace GroveGames.ObjectPool;

public sealed class HashSetPool<T> : IHashSetPool<T> where T : notnull
{
    private readonly ObjectPool<HashSet<T>> _pool;
    private bool _disposed;

    public int Count
    {
        get
        {
            ThrowHelper.ThrowIfDisposed(_disposed, this);
            return _pool.Count;
        }
    }

    public int MaxSize
    {
        get
        {
            ThrowHelper.ThrowIfDisposed(_disposed, this);
            return _pool.MaxSize;
        }
    }

    public HashSetPool(int initialSize, int maxSize, IEqualityComparer<T>? comparer = null)
    {
        ThrowHelper.ThrowIfNegative(initialSize);
        ThrowHelper.ThrowIfGreaterThan(initialSize, maxSize);
        ThrowHelper.ThrowIfNegativeOrZero(maxSize);

        _pool = new ObjectPool<HashSet<T>>(() => new HashSet<T>(comparer), null, static hashSet => hashSet.Clear(), initialSize, maxSize);
        _disposed = false;
    }

    public HashSet<T> Rent()
    {
        ThrowHelper.ThrowIfDisposed(_disposed, this);

        return _pool.Rent();
    }

    public void Return(HashSet<T> hashSet)
    {
        ThrowHelper.ThrowIfDisposed(_disposed, this);

        _pool.Return(hashSet);
    }

    public void Clear()
    {
        ThrowHelper.ThrowIfDisposed(_disposed, this);

        _pool.Clear();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _pool.Dispose();
    }
}