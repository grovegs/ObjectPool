using System;
using System.Collections.Concurrent;
using System.Threading;

namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentIndexedObjectPool<TValue> : IKeyedObjectPool<int, TValue> where TValue : class
{
    private readonly Func<int, IConcurrentObjectPool<TValue>> _factory;
    private readonly ConcurrentDictionary<int, IConcurrentObjectPool<TValue>> _pools;
    private volatile int _disposed;

    public ConcurrentIndexedObjectPool(Func<int, IConcurrentObjectPool<TValue>> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _factory = factory;
        _pools = new ConcurrentDictionary<int, IConcurrentObjectPool<TValue>>();
        _disposed = 0;
    }

    public int Count(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        if (!_pools.TryGetValue(index, out var pool))
        {
            return 0;
        }

        return pool.Count;
    }

    public int MaxSize(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        if (!_pools.TryGetValue(index, out var pool))
        {
            return 0;
        }

        return pool.MaxSize;
    }

    public TValue Rent(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        var pool = _pools.GetOrAdd(index, i =>
        {
            var newPool = _factory(i);
            ArgumentNullException.ThrowIfNull(newPool, nameof(_factory));
            return newPool;
        });

        return pool.Rent();
    }

    public void Return(int index, TValue item)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        var pool = _pools.GetOrAdd(index, i =>
        {
            var newPool = _factory(i);
            ArgumentNullException.ThrowIfNull(newPool, nameof(_factory));
            return newPool;
        });

        pool.Return(item);
    }

    public void Warm(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        var pool = _pools.GetOrAdd(index, i =>
        {
            var newPool = _factory(i);
            ArgumentNullException.ThrowIfNull(newPool, nameof(_factory));
            return newPool;
        });

        pool.Warm();
    }

    public void Warm()
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        foreach (var pool in _pools.Values)
        {
            pool.Warm();
        }
    }

    public void Clear(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        if (_pools.TryGetValue(index, out var pool))
        {
            pool.Clear();
        }
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        foreach (var pool in _pools.Values)
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

        foreach (var pool in _pools.Values)
        {
            pool.Dispose();
        }
    }
}
