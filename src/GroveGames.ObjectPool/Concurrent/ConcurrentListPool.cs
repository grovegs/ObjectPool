namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentListPool<T> : IListPool<T>
{
    private readonly ConcurrentObjectPool<List<T>> _pool;
    private volatile bool _disposed;

    public int Count => _disposed ? throw new ObjectDisposedException(nameof(ConcurrentListPool<T>)) : _pool.Count;
    public int MaxSize => _disposed ? throw new ObjectDisposedException(nameof(ConcurrentListPool<T>)) : _pool.MaxSize;

    public ConcurrentListPool(int capacity, int maxSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _pool = new ConcurrentObjectPool<List<T>>(() => new List<T>(capacity), static list => list.Clear(), maxSize);
        _disposed = false;
    }

    public List<T> Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _pool.Rent();
    }

    public void Return(List<T> list)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _pool.Return(list);
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

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
