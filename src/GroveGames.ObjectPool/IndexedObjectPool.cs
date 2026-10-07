using System;

namespace GroveGames.ObjectPool;

public sealed class IndexedObjectPool<TValue> : IKeyedObjectPool<int, TValue> where TValue : class
{
    private const int InitialCapacity = 4;

    private readonly Func<int, IObjectPool<TValue>> _factory;
    private IObjectPool<TValue>?[] _pools;
    private bool _disposed;

    public IndexedObjectPool(Func<int, IObjectPool<TValue>> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _factory = factory;
        _pools = new IObjectPool<TValue>?[InitialCapacity];
        _disposed = false;
    }

    public int Count(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        return GetPool(index)?.Count ?? 0;
    }

    public int MaxSize(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        return GetPool(index)?.MaxSize ?? 0;
    }

    public TValue Rent(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        return GetOrCreatePool(index).Rent();
    }

    public void Return(int index, TValue item)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        GetOrCreatePool(index).Return(item);
    }

    public void Warm(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        GetOrCreatePool(index).Warm();
    }

    public void Warm()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var pools = _pools;

        for (var i = 0; i < pools.Length; i++)
        {
            pools[i]?.Warm();
        }
    }

    public void Clear(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        GetPool(index)?.Clear();
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var pools = _pools;

        for (var i = 0; i < pools.Length; i++)
        {
            pools[i]?.Clear();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        var pools = _pools;

        for (var i = 0; i < pools.Length; i++)
        {
            pools[i]?.Dispose();
            pools[i] = null;
        }
    }

    private IObjectPool<TValue>? GetPool(int index)
    {
        var pools = _pools;
        return index < pools.Length ? pools[index] : null;
    }

    private IObjectPool<TValue> GetOrCreatePool(int index)
    {
        var pools = _pools;

        if (index < pools.Length)
        {
            var existing = pools[index];

            if (existing != null)
            {
                return existing;
            }
        }
        else
        {
            Array.Resize(ref _pools, Math.Max(index + 1, pools.Length * 2));
        }

        var pool = _factory(index);
        ArgumentNullException.ThrowIfNull(pool, nameof(_factory));
        _pools[index] = pool;
        return pool;
    }
}
