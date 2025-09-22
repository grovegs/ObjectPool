using System.Collections.Generic;
using System.Threading;

namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentHashSetPool<T> : IHashSetPool<T> where T : notnull
{
    private readonly ConcurrentObjectPool<HashSet<T>> _pool;
    private volatile int _disposed;

    public int Count
    {
        get
        {
            ThrowHelper.ThrowIfDisposed(_disposed == 1, this);
            return _pool.Count;
        }
    }

    public int MaxSize
    {
        get
        {
            ThrowHelper.ThrowIfDisposed(_disposed == 1, this);
            return _pool.MaxSize;
        }
    }

    public ConcurrentHashSetPool(int initialSize, int maxSize, IEqualityComparer<T>? comparer = null)
    {
        ThrowHelper.ThrowIfNegative(initialSize);
        ThrowHelper.ThrowIfGreaterThan(initialSize, maxSize);
        ThrowHelper.ThrowIfNegativeOrZero(maxSize);

        _pool = new ConcurrentObjectPool<HashSet<T>>(
            () => new HashSet<T>(comparer),
            null,
            static set => set.Clear(),
            initialSize,
            maxSize);
        _disposed = 0;
    }

    public HashSet<T> Rent()
    {
        ThrowHelper.ThrowIfDisposed(_disposed == 1, this);

        return _pool.Rent();
    }

    public void Return(HashSet<T> hashSet)
    {
        ThrowHelper.ThrowIfDisposed(_disposed == 1, this);

        _pool.Return(hashSet);
    }

    public void Clear()
    {
        ThrowHelper.ThrowIfDisposed(_disposed == 1, this);

        _pool.Clear();
    }

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
        {
            return;
        }

        _pool.Dispose();
    }
}