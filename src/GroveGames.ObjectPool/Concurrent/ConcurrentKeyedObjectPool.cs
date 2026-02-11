using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentKeyedObjectPool<TKey, TValue> : IKeyedObjectPool<TKey, TValue>
    where TKey : notnull
    where TValue : class
{
    private readonly Func<TKey, IConcurrentObjectPool<TValue>> _factory;
    private readonly ConcurrentDictionary<TKey, IConcurrentObjectPool<TValue>> _pools;
    private volatile int _disposed;

    public ConcurrentKeyedObjectPool(Func<TKey, IConcurrentObjectPool<TValue>> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _factory = factory;
        _pools = new ConcurrentDictionary<TKey, IConcurrentObjectPool<TValue>>();
        _disposed = 0;
    }

    public int Count(TKey key)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        if (!_pools.TryGetValue(key, out var pool))
        {
            return 0;
        }

        return pool.Count;
    }

    public int MaxSize(TKey key)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        if (!_pools.TryGetValue(key, out var pool))
        {
            return 0;
        }

        return pool.MaxSize;
    }

    public TValue Rent(TKey key)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        var pool = _pools.GetOrAdd(key, k =>
        {
            var newPool = _factory(k);
            ArgumentNullException.ThrowIfNull(newPool, nameof(_factory));
            return newPool;
        });

        return pool.Rent();
    }

    public void Return(TKey key, TValue item)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        var pool = _pools.GetOrAdd(key, k =>
        {
            var newPool = _factory(k);
            ArgumentNullException.ThrowIfNull(newPool, nameof(_factory));
            return newPool;
        });

        pool.Return(item);
    }

    public void Warm(TKey key)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        var pool = _pools.GetOrAdd(key, k =>
        {
            var newPool = _factory(k);
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

    public void Clear(TKey key)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        if (_pools.TryGetValue(key, out var pool))
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
