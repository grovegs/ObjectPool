using System.Collections.Generic;
using System.Threading;

namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentListPool<T> : IListPool<T> where T : notnull
{
    private readonly ConcurrentObjectPool<List<T>> _pool;
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

    public ConcurrentListPool(int initialSize, int maxSize)
    {
        ThrowHelper.ThrowIfNegative(initialSize);
        ThrowHelper.ThrowIfGreaterThan(initialSize, maxSize);
        ThrowHelper.ThrowIfNegativeOrZero(maxSize);

        _pool = new ConcurrentObjectPool<List<T>>(
            static () => [],
            null,
            static list => list.Clear(),
            initialSize,
            maxSize);
        _disposed = 0;
    }

    public List<T> Rent()
    {
        ThrowHelper.ThrowIfDisposed(_disposed == 1, this);

        return _pool.Rent();
    }

    public void Return(List<T> list)
    {
        ThrowHelper.ThrowIfDisposed(_disposed == 1, this);

        _pool.Return(list);
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