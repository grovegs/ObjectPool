namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentQueuePool<T> : IQueuePool<T> where T : class
{
    private readonly ConcurrentObjectPool<Queue<T>> _pool;
    private volatile int _disposed;

    public int Count => _disposed == 1 ? throw new ObjectDisposedException(nameof(ConcurrentQueuePool<T>)) : _pool.Count;
    public int MaxSize => _disposed == 1 ? throw new ObjectDisposedException(nameof(ConcurrentQueuePool<T>)) : _pool.MaxSize;

    public ConcurrentQueuePool(int initialSize, int maxSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, maxSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _pool = new ConcurrentObjectPool<Queue<T>>(
            static () => new Queue<T>(),
            null,
            static queue => queue.Clear(),
            initialSize,
            maxSize);
        _disposed = 0;
    }

    public Queue<T> Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        return _pool.Rent();
    }

    public void Return(Queue<T> queue)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        _pool.Return(queue);
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

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