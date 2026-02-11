using System;
using System.Collections.Generic;

namespace GroveGames.ObjectPool;

public sealed class DictionaryPool<TKey, TValue> : IObjectPool<Dictionary<TKey, TValue>> where TKey : notnull
{
    private readonly ObjectPool<Dictionary<TKey, TValue>> _pool;
    private bool _disposed;

    public int Count
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _pool.Count;
        }
    }

    public int MaxSize
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _pool.MaxSize;
        }
    }

    public DictionaryPool(int initialSize, int maxSize, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, maxSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _pool = new ObjectPool<Dictionary<TKey, TValue>>(
            () => new Dictionary<TKey, TValue>(comparer),
            null,
            static dictionary => dictionary.Clear(),
            initialSize,
            maxSize);
        _disposed = false;
    }

    public Dictionary<TKey, TValue> Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _pool.Rent();
    }

    public void Return(Dictionary<TKey, TValue> dictionary)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _pool.Return(dictionary);
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _pool.Clear();
    }

    public void Warm()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _pool.Warm();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _pool.Dispose();
    }
}
