namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentQueuePool<T> : IQueuePool<T> where T : class
{
    private readonly ConcurrentObjectPool<Queue<T>> _pool;
    private volatile bool _disposed;

    public int Count => _disposed ? throw new ObjectDisposedException(nameof(ConcurrentQueuePool<T>)) : _pool.Count;
    public int MaxSize => _disposed ? throw new ObjectDisposedException(nameof(ConcurrentQueuePool<T>)) : _pool.MaxSize;

    public ConcurrentQueuePool(int initialSize, int maxSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, maxSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _pool = new ConcurrentObjectPool<Queue<T>>(static () => new Queue<T>(), null, static queue => queue.Clear(), initialSize, maxSize);
        _disposed = false;
    }

    public Queue<T> Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _pool.Rent();
    }

    public void Return(Queue<T> queue)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _pool.Return(queue);
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
