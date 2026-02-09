using System;
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
            ObjectDisposedException.ThrowIf(_disposed == 1, this);
            return _pool.Count;
        }
    }

    public int MaxSize
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed == 1, this);
            return _pool.MaxSize;
        }
    }

    public ConcurrentListPool(int initialSize, int maxSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, maxSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

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
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        return _pool.Rent();
    }

    public void Return(List<T> list)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        _pool.Return(list);
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

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
