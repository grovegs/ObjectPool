using System;
using System.Collections.Generic;

namespace GroveGames.ObjectPool;

public sealed class KeyedObjectPool<TKey, TValue> : IKeyedObjectPool<TKey, TValue>
    where TKey : notnull
    where TValue : class
{
    private readonly Dictionary<TKey, IObjectPool<TValue>> _pools;
    private bool _disposed;

    public KeyedObjectPool(IDictionary<TKey, IObjectPool<TValue>> pools)
    {
        ArgumentNullException.ThrowIfNull(pools);

        _pools = new Dictionary<TKey, IObjectPool<TValue>>(pools);

        foreach (var kvp in _pools)
        {
            ArgumentNullException.ThrowIfNull(kvp.Value, nameof(pools));
        }

        _disposed = false;
    }

    public int Count(TKey key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_pools.TryGetValue(key, out var pool))
        {
            return 0;
        }

        return pool.Count;
    }

    public int MaxSize(TKey key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_pools.TryGetValue(key, out var pool))
        {
            return 0;
        }

        return pool.MaxSize;
    }

    public TValue Rent(TKey key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_pools.TryGetValue(key, out var pool))
        {
            throw new KeyNotFoundException($"No pool registered for key: {key}");
        }

        return pool.Rent();
    }

    public void Return(TKey key, TValue item)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_pools.TryGetValue(key, out var pool))
        {
            pool.Return(item);
        }
    }

    public void Warm(TKey key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_pools.TryGetValue(key, out var pool))
        {
            throw new KeyNotFoundException($"No pool registered for key: {key}");
        }

        pool.Warm();
    }

    public void Warm()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        foreach (var pool in _pools.Values)
        {
            pool.Warm();
        }
    }

    public void Clear(TKey key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_pools.TryGetValue(key, out var pool))
        {
            pool.Clear();
        }
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        foreach (var pool in _pools.Values)
        {
            pool.Clear();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        foreach (var pool in _pools.Values)
        {
            pool.Dispose();
        }
    }
}
