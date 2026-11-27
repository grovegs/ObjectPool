using System;
using System.Collections.Generic;

namespace GroveGames.ObjectPool;

public sealed class ArrayPool<T> : IArrayPool<T> where T : notnull
{
    private readonly Dictionary<int, ObjectPool<T[]>> _poolsBySize;
    private readonly int _initialSize;
    private readonly int _maxSize;
    private readonly bool _clearOnReturn;
    private bool _disposed;

    public ArrayPool(int initialSize, int maxSize)
        : this(initialSize, maxSize, false)
    {
    }

    public ArrayPool(int initialSize, int maxSize, bool clearOnReturn)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, maxSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _poolsBySize = [];
        _initialSize = initialSize;
        _maxSize = maxSize;
        _clearOnReturn = clearOnReturn;
        _disposed = false;
    }

    public int Count(int size)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _poolsBySize.TryGetValue(size, out var pool) ? pool.Count : 0;
    }

    public int MaxSize(int size)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _poolsBySize.ContainsKey(size) || size > 0 ? _maxSize : 0;
    }

    public T[] Rent(int size)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

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
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(array);

        var size = array.Length;

        if (size == 0)
        {
            return;
        }

        if (!_poolsBySize.TryGetValue(size, out var pool))
        {
            pool = AddPool(size);
        }

        if (_clearOnReturn && pool.Count < pool.MaxSize)
        {
            Array.Clear(array, 0, array.Length);
        }

        pool.Return(array);
    }

    private ObjectPool<T[]> AddPool(int size)
    {
        var pool = new ObjectPool<T[]>(() => new T[size], null, null, _initialSize, _maxSize);
        _poolsBySize[size] = pool;
        return pool;
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        foreach (var pool in _poolsBySize.Values)
        {
            pool.Clear();
        }

        _poolsBySize.Clear();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        foreach (var pool in _poolsBySize.Values)
        {
            pool.Dispose();
        }

        _poolsBySize.Clear();
    }
}
