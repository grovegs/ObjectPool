using System;
using System.Collections.Generic;

namespace GroveGames.ObjectPool;

public sealed class IndexedObjectPool<TValue> : IKeyedObjectPool<int, TValue> where TValue : class
{
    private readonly Func<int, IObjectPool<TValue>> _factory;
    private readonly Dictionary<int, IObjectPool<TValue>> _pools;
    private bool _disposed;

    public IndexedObjectPool(Func<int, IObjectPool<TValue>> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _factory = factory;
        _pools = [];
        _disposed = false;
    }

    public int Count(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        if (!_pools.TryGetValue(index, out var pool))
        {
            return 0;
        }

        return pool.Count;
    }

    public int MaxSize(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        if (!_pools.TryGetValue(index, out var pool))
        {
            return 0;
        }

        return pool.MaxSize;
    }

    public TValue Rent(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        if (!_pools.TryGetValue(index, out var pool))
        {
            pool = _factory(index);
            ArgumentNullException.ThrowIfNull(pool, nameof(_factory));
            _pools[index] = pool;
        }

        return pool.Rent();
    }

    public void Return(int index, TValue item)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        if (!_pools.TryGetValue(index, out var pool))
        {
            pool = _factory(index);
            ArgumentNullException.ThrowIfNull(pool, nameof(_factory));
            _pools[index] = pool;
        }

        pool.Return(item);
    }

    public void Warm(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        if (!_pools.TryGetValue(index, out var pool))
        {
            pool = _factory(index);
            ArgumentNullException.ThrowIfNull(pool, nameof(_factory));
            _pools[index] = pool;
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

    public void Clear(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        if (_pools.TryGetValue(index, out var pool))
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
