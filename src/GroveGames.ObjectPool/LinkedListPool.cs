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

    public LinkedListPool(int initialSize, int maxSize)
    {
        ThrowHelper.ThrowIfNegative(initialSize);
        ThrowHelper.ThrowIfGreaterThan(initialSize, maxSize);
        ThrowHelper.ThrowIfNegativeOrZero(maxSize);

        _pool = new ObjectPool<LinkedList<T>>(static () => new LinkedList<T>(), null, static linkedList => linkedList.Clear(), initialSize, maxSize);
        _disposed = false;
    }

    public LinkedList<T> Rent()
    {
        ThrowHelper.ThrowIfDisposed(_disposed, this);

        return _pool.Rent();
    }

    public void Return(LinkedList<T> linkedList)
    {
        ThrowHelper.ThrowIfDisposed(_disposed, this);

        _pool.Return(linkedList);
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