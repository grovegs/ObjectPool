using System;
using System.Threading;

namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentIndexedObjectPool<TValue> : IIndexedObjectPool<TValue> where TValue : class
{
    private readonly ConcurrentObjectPool<TValue>[] _pools;
    private volatile int _disposed;

    public ConcurrentIndexedObjectPool(int poolCount, Func<int, TValue> factory, Action<TValue>? onRent, Action<TValue>? onReturn, int initialSize, int maxSize)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(poolCount);
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, maxSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _pools = new ConcurrentObjectPool<TValue>[poolCount];

        for (int i = 0; i < poolCount; i++)
        {
            _pools[i] = new ConcurrentObjectPool<TValue>(() => factory(i), onRent, onReturn, initialSize, maxSize);
        }

        _disposed = 0;
    }

    public int Count(int key)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        if (key < 0 || key >= _pools.Length)
        {
            return 0;
        }

        return _pools[key].Count;
    }

    public int MaxSize(int key)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        if (key < 0 || key >= _pools.Length)
        {
            return 0;
        }

        return _pools[key].MaxSize;
    }

    public TValue Rent(int key)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        if (key < 0 || key >= _pools.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(key));
        }

        return _pools[key].Rent();
    }

    public void Return(int key, TValue item)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        if (key >= 0 && key < _pools.Length)
        {
            _pools[key].Return(item);
        }
    }

    public void Warm(int key)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        if (key < 0 || key >= _pools.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(key));
        }

        _pools[key].Warm();
    }

    public void Warm()
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        foreach (var pool in _pools)
        {
            pool.Warm();
        }
    }

    public void Clear(int key)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        if (key >= 0 && key < _pools.Length)
        {
            _pools[key].Clear();
        }
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        foreach (var pool in _pools)
        {
            pool.Clear();
        }
    }

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
        {
            return;
        }

        foreach (var pool in _pools)
        {
            pool.Dispose();
        }
    }
}
