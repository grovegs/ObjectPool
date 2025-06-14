namespace GroveGames.ObjectPool;

public sealed class ArrayPool<T> : IArrayPool<T> where T : notnull
{
    private readonly Dictionary<int, Queue<T[]>> _poolsBySize;
    private readonly int _maxSize;
    private bool _disposed;

    public ArrayPool(int maxSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _poolsBySize = [];
        _maxSize = maxSize;
        _disposed = false;
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

        if (!_poolsBySize.TryGetValue(size, out var queue))
        {
            queue = new Queue<T[]>();
            _poolsBySize[size] = queue;
        }

        if (queue.Count < _maxSize)
        {
            queue.Enqueue(array);
        }
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