using System.Collections.Concurrent;

namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentArrayPool<T> : IArrayPool<T> where T : notnull
{
    private readonly ConcurrentDictionary<int, ConcurrentObjectPool<T[]>> _poolsBySize;
    private readonly int _maxSize;
    private volatile bool _disposed;

    public ConcurrentArrayPool(int maxSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _poolsBySize = new ConcurrentDictionary<int, ConcurrentObjectPool<T[]>>();
        _maxSize = maxSize;
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

        var pool = _poolsBySize.GetOrAdd(size, new ConcurrentObjectPool<T[]>(() => new T[size], null, _maxSize));
        return pool.Rent();
    }

    public void Return(T[] array, bool clearArray = false)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (clearArray)
        {
            Array.Clear(array);
        }

        var size = array.Length;
        var pool = _poolsBySize.GetOrAdd(size, new ConcurrentObjectPool<T[]>(() => new T[size], null, _maxSize));
        pool.Return(array);
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