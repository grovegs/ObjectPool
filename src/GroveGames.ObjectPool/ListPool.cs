using System;
using System.Collections.Generic;

namespace GroveGames.ObjectPool;

public sealed class ListPool<T> : IListPool<T> where T : notnull
{
    private readonly ObjectPool<List<T>> _pool;
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

    public ListPool(int initialSize, int maxSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, maxSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _pool = new ObjectPool<List<T>>(static () => [], null, static list => list.Clear(), initialSize, maxSize);
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
