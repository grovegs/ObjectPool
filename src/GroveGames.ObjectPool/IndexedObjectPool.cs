using System;

namespace GroveGames.ObjectPool;

public sealed class IndexedObjectPool<TValue> : IIndexedObjectPool<TValue> where TValue : class
{
    private readonly ObjectPool<TValue>[] _pools;
    private bool _disposed;

    public IndexedObjectPool(int poolCount, Func<int, TValue> factory, Action<TValue>? onRent, Action<TValue>? onReturn, int initialSize, int maxSize)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(poolCount);
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, maxSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _pools = new ObjectPool<TValue>[poolCount];

        for (int i = 0; i < poolCount; i++)
        {
            _pools[i] = new ObjectPool<TValue>(() => factory(i), onRent, onReturn, initialSize, maxSize);
        }

        _disposed = false;
    }

    public int Count(int key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (key < 0 || key >= _pools.Length)
        {
            return 0;
        }

        return _pools[key].Count;
    }

    public int MaxSize(int key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (key < 0 || key >= _pools.Length)
        {
            return 0;
        }

        return _pools[key].MaxSize;
    }

    public TValue Rent(int key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (key < 0 || key >= _pools.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(key));
        }

        return _pools[key].Rent();
    }

    public void Return(int key, TValue item)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (key >= 0 && key < _pools.Length)
        {
            _pools[key].Return(item);
        }
    }

    public void Warm(int key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (key < 0 || key >= _pools.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(key));
        }

        _pools[key].Warm();
    }

    public void Warm()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        foreach (var pool in _pools)
        {
            pool.Warm();
        }
    }

    public void Clear(int key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (key >= 0 && key < _pools.Length)
        {
            _pools[key].Clear();
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
