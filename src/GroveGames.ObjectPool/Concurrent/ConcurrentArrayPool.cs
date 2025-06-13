using System.Collections.Concurrent;

namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentArrayPool<T> : IArrayPool<T>
{
    private readonly ConcurrentDictionary<int, ConcurrentQueue<T[]>> _poolsBySize;
    private readonly int _maxSize;
    private volatile bool _disposed;

    public ConcurrentArrayPool(int maxSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _poolsBySize = new ConcurrentDictionary<int, ConcurrentQueue<T[]>>();
        _maxSize = maxSize;
        _disposed = false;
    }

    public T[] Rent(int size)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (size == 0)
        {
            return [];
        }

        if (_poolsBySize.TryGetValue(size, out var queue) && queue.TryDequeue(out var array))
        {
            return array;
        }

        return new T[size];
    }

    public void Return(T[] array, bool clearArray = false)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (clearArray)
        {
            Array.Clear(array);
        }

        var size = array.Length;
        var queue = _poolsBySize.GetOrAdd(size, _ => new ConcurrentQueue<T[]>());

        if (queue.Count < _maxSize)
        {
            queue.Enqueue(array);
        }
    }

    public int Count(int size)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _poolsBySize.TryGetValue(size, out var queue) ? queue.Count : 0;
    }

    public int MaxSize(int size)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _poolsBySize.ContainsKey(size) || size > 0 ? _maxSize : 0;
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        foreach (var queue in _poolsBySize.Values)
        {
            queue.Clear();
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

        foreach (var queue in _poolsBySize.Values)
        {
            queue.Clear();
        }

        _poolsBySize.Clear();
    }
}
