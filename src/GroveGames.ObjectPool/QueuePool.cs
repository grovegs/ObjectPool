namespace GroveGames.ObjectPool;

public sealed class QueuePool<T> : IQueuePool<T> where T : notnull
{
    private readonly ObjectPool<Queue<T>> _pool;
    private bool _disposed;

    public int Count => _disposed ? throw new ObjectDisposedException(nameof(QueuePool<T>)) : _pool.Count;
    public int MaxSize => _disposed ? throw new ObjectDisposedException(nameof(QueuePool<T>)) : _pool.MaxSize;

    public QueuePool(int maxSize, int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);

        _pool = new ObjectPool<Queue<T>>(() => new Queue<T>(capacity), static queue => queue.Clear(), maxSize);
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