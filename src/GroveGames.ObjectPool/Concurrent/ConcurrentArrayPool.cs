using System;
using System.Collections.Concurrent;
using System.Threading;

namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentArrayPool<T> : IArrayPool<T> where T : notnull
{
    private readonly ConcurrentDictionary<int, ConcurrentObjectPool<T[]>> _poolsBySize;
    private readonly int _initialSize;
    private readonly int _maxSize;
    private readonly bool _clearOnReturn;
    private volatile int _disposed;

    public ConcurrentArrayPool(int initialSize, int maxSize)
        : this(initialSize, maxSize, false)
    {
    }

    public ConcurrentArrayPool(int initialSize, int maxSize, bool clearOnReturn)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, maxSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _poolsBySize = new ConcurrentDictionary<int, ConcurrentObjectPool<T[]>>();
        _initialSize = initialSize;
        _maxSize = maxSize;
        _clearOnReturn = clearOnReturn;
        _disposed = 0;
    }

    public int Count(int size)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        return _poolsBySize.TryGetValue(size, out var pool) ? pool.Count : 0;
    }

    public int MaxSize(int size)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        return _poolsBySize.ContainsKey(size) || size > 0 ? _maxSize : 0;
    }

    public T[] Rent(int size)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        if (size == 0)
        {
            return [];
        }

        if (!_poolsBySize.TryGetValue(size, out var pool))
        {
            pool = AddPool(size);
        }

        return pool.Rent();
    }

    public void Return(T[] array)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);
        ArgumentNullException.ThrowIfNull(array);

        if (!_poolsBySize.TryGetValue(array.Length, out var pool))
        {
            return;
        }

        if (_clearOnReturn && pool.Count < pool.MaxSize)
        {
            Array.Clear(array, 0, array.Length);
        }

        pool.Return(array);
    }

    private ConcurrentObjectPool<T[]> AddPool(int size)
    {
        var created = new ConcurrentObjectPool<T[]>(() => new T[size], null, null, _initialSize, _maxSize);
        var pool = _poolsBySize.GetOrAdd(size, created);

        if (!ReferenceEquals(pool, created))
        {
            created.Dispose();
        }

        return pool;
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        foreach (var kvp in _poolsBySize)
        {
            kvp.Value.Clear();
        }

        _poolsBySize.Clear();
    }

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
        {
            return;
        }

        foreach (var kvp in _poolsBySize)
        {
            kvp.Value.Dispose();
        }

        _poolsBySize.Clear();
    }
}
