using System.Collections.Generic;

namespace GroveGames.ObjectPool;

public sealed class DictionaryPool<TKey, TValue> : IDictionaryPool<TKey, TValue> where TKey : notnull
{
    private readonly ObjectPool<Dictionary<TKey, TValue>> _pool;
    private bool _disposed;

    public int Count
    {
        get
        {
            ThrowHelper.ThrowIfDisposed(_disposed, this);
            return _pool.Count;
        }
    }

    public int MaxSize
    {
        get
        {
            ThrowHelper.ThrowIfDisposed(_disposed, this);
            return _pool.MaxSize;
        }
    }

    public DictionaryPool(int initialSize, int maxSize, IEqualityComparer<TKey>? comparer = null)
    {
        ThrowHelper.ThrowIfNegative(initialSize);
        ThrowHelper.ThrowIfGreaterThan(initialSize, maxSize);
        ThrowHelper.ThrowIfNegativeOrZero(maxSize);

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
        ThrowHelper.ThrowIfDisposed(_disposed, this);

        return _pool.Rent();
    }

    public void Return(Dictionary<TKey, TValue> dictionary)
    {
        ThrowHelper.ThrowIfDisposed(_disposed, this);

        _pool.Return(dictionary);
    }

    public void Clear()
    {
        ThrowHelper.ThrowIfDisposed(_disposed, this);

        _pool.Clear();
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