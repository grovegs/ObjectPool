using System;
using System.Collections.Generic;

namespace GroveGames.ObjectPool;

public sealed class LinkedListPool<T> : ILinkedListPool<T> where T : notnull
{
    private readonly ObjectPool<LinkedList<T>> _pool;
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

    public LinkedListPool(int initialSize, int maxSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, maxSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _pool = new ObjectPool<LinkedList<T>>(static () => new LinkedList<T>(), null, static linkedList => linkedList.Clear(), initialSize, maxSize);
        _disposed = false;
    }

    public LinkedList<T> Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _pool.Rent();
    }

    public void Return(LinkedList<T> linkedList)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _pool.Return(linkedList);
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