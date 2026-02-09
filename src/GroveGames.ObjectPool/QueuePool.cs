using System;
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
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _pool.Count;
        }
    }

    public int MaxSize
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _pool.MaxSize;
        }
    }

    public QueuePool(int initialSize, int maxSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, maxSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _pool = new ObjectPool<Queue<T>>(static () => new Queue<T>(), null, static queue => queue.Clear(), initialSize, maxSize);
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
