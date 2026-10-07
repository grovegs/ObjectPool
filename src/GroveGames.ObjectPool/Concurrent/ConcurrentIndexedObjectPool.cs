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

        return _pools.TryGetValue(index, out var pool) ? pool.Count : 0;
    }

    public int MaxSize(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        return _pools.TryGetValue(index, out var pool) ? pool.MaxSize : 0;
    }

    public TValue Rent(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        return GetOrCreatePool(index).Rent();
    }

    public void Return(int index, TValue item)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        GetOrCreatePool(index).Return(item);
    }

    public void Warm(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        GetOrCreatePool(index).Warm();
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

        _pools.Clear();
    }

    private IConcurrentObjectPool<TValue> GetOrCreatePool(int index)
    {
        if (_pools.TryGetValue(index, out var pool))
        {
            return pool;
        }

        var created = _factory(index);
        ArgumentNullException.ThrowIfNull(created, nameof(_factory));
        pool = _pools.GetOrAdd(index, created);

        if (!ReferenceEquals(pool, created))
        {
            created.Dispose();
        }

        return pool;
    }
}
