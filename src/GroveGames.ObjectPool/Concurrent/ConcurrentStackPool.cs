using System;
using System.Collections.Generic;
using System.Threading;

namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentStackPool<T> : IStackPool<T> where T : notnull
{
    private readonly ConcurrentObjectPool<Stack<T>> _pool;
    private volatile int _disposed;

    public int Count
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed == 1, this);
            return _pool.Count;
        }
    }

    public int MaxSize
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed == 1, this);
            return _pool.MaxSize;
        }
    }

    public ConcurrentStackPool(int initialSize, int maxSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, maxSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _pool = new ConcurrentObjectPool<Stack<T>>(
            static () => new Stack<T>(),
            null,
            static stack => stack.Clear(),
            initialSize,
            maxSize);
        _disposed = 0;
    }

    public Stack<T> Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        return _pool.Rent();
    }

    public void Return(Stack<T> stack)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        _pool.Return(stack);
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        _pool.Clear();
    }

    public void Warm()
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        _pool.Warm();
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
