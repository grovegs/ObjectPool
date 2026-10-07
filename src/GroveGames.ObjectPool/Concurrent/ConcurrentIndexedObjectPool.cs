using System;
using System.Threading;

namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentIndexedObjectPool<TValue> : IKeyedObjectPool<int, TValue> where TValue : class
{
    private const int InitialCapacity = 4;

    private readonly Func<int, IConcurrentObjectPool<TValue>> _factory;
    private readonly object _lock;
    private IConcurrentObjectPool<TValue>?[] _pools;
    private volatile int _disposed;

    public ConcurrentIndexedObjectPool(Func<int, IConcurrentObjectPool<TValue>> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _factory = factory;
        _lock = new object();
        _pools = new IConcurrentObjectPool<TValue>?[InitialCapacity];
        _disposed = 0;
    }

    public int Count(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        return GetPool(index)?.Count ?? 0;
    }

    public int MaxSize(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        return GetPool(index)?.MaxSize ?? 0;
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

        var pools = Volatile.Read(ref _pools);

        for (var i = 0; i < pools.Length; i++)
        {
            Volatile.Read(ref pools[i])?.Warm();
        }
    }

    public void Clear(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        GetPool(index)?.Clear();
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        var pools = Volatile.Read(ref _pools);

        for (var i = 0; i < pools.Length; i++)
        {
            Volatile.Read(ref pools[i])?.Clear();
        }
    }

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
        {
            return;
        }

        lock (_lock)
        {
            var pools = _pools;

            for (var i = 0; i < pools.Length; i++)
            {
                pools[i]?.Dispose();
                pools[i] = null;
            }
        }
    }

    private IConcurrentObjectPool<TValue>? GetPool(int index)
    {
        var pools = Volatile.Read(ref _pools);
        return index < pools.Length ? Volatile.Read(ref pools[index]) : null;
    }

    private IConcurrentObjectPool<TValue> GetOrCreatePool(int index)
    {
        var existing = GetPool(index);

        if (existing != null)
        {
            return existing;
        }

        lock (_lock)
        {
            var pools = _pools;

            if (index >= pools.Length)
            {
                var grown = new IConcurrentObjectPool<TValue>?[Math.Max(index + 1, pools.Length * 2)];
                Array.Copy(pools, grown, pools.Length);
                Volatile.Write(ref _pools, grown);
                pools = grown;
            }

            var pool = pools[index];

            if (pool != null)
            {
                return pool;
            }

            pool = _factory(index);
            ArgumentNullException.ThrowIfNull(pool, nameof(_factory));
            Volatile.Write(ref pools[index], pool);
            return pool;
        }
    }
}
