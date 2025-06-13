namespace GroveGames.ObjectPool;

public sealed class FixedSizeArrayPool<T> : IArrayPool<T>
{
    private readonly Dictionary<int, Queue<T[]>> _poolsBySize;
    private readonly int _maxSize;
    private bool _disposed;

    public FixedSizeArrayPool(int maxSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _poolsBySize = [];
        _maxSize = maxSize;
        _disposed = false;
    }

    public T[] Rent(int size)
    {
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
        if (_disposed)
        {
            return;
        }

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

    public int Count(int size)
    {
        return _poolsBySize.TryGetValue(size, out var queue) ? queue.Count : 0;
    }

    public int MaxSize(int size)
    {
        return _poolsBySize.ContainsKey(size) || size > 0 ? _maxSize : 0;
    }

    public void Clear()
    {
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