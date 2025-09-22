using System.Collections.Generic;

namespace GroveGames.ObjectPool;

public sealed class QueuePool<T> : IQueuePool<T> where T : notnull
{
    private readonly ObjectPool<Queue<T>> _pool;
    private bool _disposed;

    public int Count
    {
        get
        {
            ThrowHelper.ThrowIfDisposed(_disposed, this);
            return _pool.Count;
        }
    }

    public int MaxSize
    {
        get
        {
            ThrowHelper.ThrowIfDisposed(_disposed, this);
            return _pool.MaxSize;
        }
    }

    public QueuePool(int initialSize, int maxSize)
    {
        ThrowHelper.ThrowIfNegative(initialSize);
        ThrowHelper.ThrowIfGreaterThan(initialSize, maxSize);
        ThrowHelper.ThrowIfNegativeOrZero(maxSize);

        _pool = new ObjectPool<Queue<T>>(static () => new Queue<T>(), null, static queue => queue.Clear(), initialSize, maxSize);
        _disposed = false;
    }

    public Queue<T> Rent()
    {
        ThrowHelper.ThrowIfDisposed(_disposed, this);

        return _pool.Rent();
    }

    public void Return(Queue<T> queue)
    {
        ThrowHelper.ThrowIfDisposed(_disposed, this);

        _pool.Return(queue);
    }

    public void Clear()
    {
        ThrowHelper.ThrowIfDisposed(_disposed, this);

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