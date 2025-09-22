using System.Collections.Generic;
using System.Threading;

namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentDictionaryPool<TKey, TValue> : IDictionaryPool<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentObjectPool<Dictionary<TKey, TValue>> _pool;
    private volatile int _disposed;

    public int Count
    {
        get
        {
            ThrowHelper.ThrowIfDisposed(_disposed == 1, this);
            return _pool.Count;
        }
    }

    public int MaxSize
    {
        get
        {
            ThrowHelper.ThrowIfDisposed(_disposed == 1, this);
            return _pool.MaxSize;
        }
    }

    public ConcurrentDictionaryPool(int initialSize, int maxSize, IEqualityComparer<TKey>? comparer = null)
    {
        ThrowHelper.ThrowIfNegative(initialSize);
        ThrowHelper.ThrowIfGreaterThan(initialSize, maxSize);
        ThrowHelper.ThrowIfNegativeOrZero(maxSize);

        _pool = new ConcurrentObjectPool<Dictionary<TKey, TValue>>(
            () => new Dictionary<TKey, TValue>(comparer),
            null,
            static dictionary => dictionary.Clear(),
            initialSize,
            maxSize);
        _disposed = 0;
    }

    public Dictionary<TKey, TValue> Rent()
    {
        ThrowHelper.ThrowIfDisposed(_disposed == 1, this);

        return _pool.Rent();
    }

    public void Return(Dictionary<TKey, TValue> dictionary)
    {
        ThrowHelper.ThrowIfDisposed(_disposed == 1, this);

        _pool.Return(dictionary);
    }

    public void Clear()
    {
        ThrowHelper.ThrowIfDisposed(_disposed == 1, this);

        _pool.Clear();
    }

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
        {
            return;
        }

        _pool.Dispose();
    }
}