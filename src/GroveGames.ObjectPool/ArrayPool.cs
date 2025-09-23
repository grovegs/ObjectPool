using System.Collections.Generic;

namespace GroveGames.ObjectPool;

public sealed class ArrayPool<T> : IArrayPool<T> where T : notnull
{
    private readonly Dictionary<int, ObjectPool<T[]>> _poolsBySize;
    private readonly int _initialSize;
    private readonly int _maxSize;
    private bool _disposed;

    public ArrayPool(int initialSize, int maxSize)
    {
        ThrowHelper.ThrowIfNegative(initialSize);
        ThrowHelper.ThrowIfGreaterThan(initialSize, maxSize);
        ThrowHelper.ThrowIfNegativeOrZero(maxSize);

        _poolsBySize = [];
        _initialSize = initialSize;
        _maxSize = maxSize;
        _disposed = false;
    }

    public int Count(int size)
    {
        ThrowHelper.ThrowIfDisposed(_disposed, this);

        return _poolsBySize.TryGetValue(size, out var pool) ? pool.Count : 0;
    }

    public int MaxSize(int size)
    {
        ThrowHelper.ThrowIfDisposed(_disposed, this);

        return _poolsBySize.ContainsKey(size) || size > 0 ? _maxSize : 0;
    }

    public T[] Rent(int size)
    {
        ThrowHelper.ThrowIfDisposed(_disposed, this);

        if (size == 0)
        {
            return [];
        }

        if (!_poolsBySize.TryGetValue(size, out var pool))
        {
            pool = new ObjectPool<T[]>(() => new T[size], null, null, _initialSize, _maxSize);
            _poolsBySize[size] = pool;
        }

        return pool.Rent();
    }

    public void Return(T[] array)
    {
        ThrowHelper.ThrowIfDisposed(_disposed, this);
        ThrowHelper.ThrowIfNull(array);

        var size = array.Length;

        if (!_poolsBySize.TryGetValue(size, out var pool))
        {
            pool = new ObjectPool<T[]>(() => new T[size], null, null, _initialSize, _maxSize);
            _poolsBySize[size] = pool;
        }

        pool.Return(array);
    }

    public void Clear()
    {
        ThrowHelper.ThrowIfDisposed(_disposed, this);

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