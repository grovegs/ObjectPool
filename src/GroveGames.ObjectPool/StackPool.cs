using System;
using System.Collections.Generic;

namespace GroveGames.ObjectPool;

public sealed class StackPool<T> : IStackPool<T> where T : notnull
{
    private readonly ObjectPool<Stack<T>> _pool;
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

    public StackPool(int initialSize, int maxSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, maxSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _pool = new ObjectPool<Stack<T>>(static () => new Stack<T>(), null, static stack => stack.Clear(), initialSize, maxSize);
        _disposed = false;
    }

    public Stack<T> Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _pool.Rent();
    }

    public void Return(Stack<T> stack)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _pool.Return(stack);
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
