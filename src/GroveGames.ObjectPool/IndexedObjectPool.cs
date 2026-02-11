using System;

namespace GroveGames.ObjectPool;

public sealed class IndexedObjectPool<TValue> : IKeyedObjectPool<int, TValue> where TValue : class
{
    private readonly IObjectPool<TValue>[] _pools;
    private bool _disposed;

    public IndexedObjectPool(int count, Func<int, IObjectPool<TValue>> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        _pools = new IObjectPool<TValue>[count];

        for (var i = 0; i < count; i++)
        {
            _pools[i] = factory(i);
            ArgumentNullException.ThrowIfNull(_pools[i], nameof(factory));
        }

        _disposed = false;
    }

    public int Count(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (index < 0 || index >= _pools.Length)
        {
            return 0;
        }

        return _pools[index].Count;
    }

    public int MaxSize(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (index < 0 || index >= _pools.Length)
        {
            return 0;
        }

        return _pools[index].MaxSize;
    }

    public TValue Rent(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (index < 0 || index >= _pools.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        return _pools[index].Rent();
    }

    public void Return(int index, TValue item)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (index >= 0 && index < _pools.Length)
        {
            _pools[index].Return(item);
        }
    }

    public void Warm(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (index < 0 || index >= _pools.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _pools[index].Warm();
    }

    public void Warm()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        foreach (var pool in _pools)
        {
            pool.Warm();
        }
    }

    public void Clear(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (index >= 0 && index < _pools.Length)
        {
            _pools[index].Clear();
        }
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        foreach (var pool in _pools)
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

        foreach (var pool in _pools)
        {
            pool.Dispose();
        }
    }
}
