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

    public ListPool(int initialSize, int maxSize)
    {
        ThrowHelper.ThrowIfNegative(initialSize);
        ThrowHelper.ThrowIfGreaterThan(initialSize, maxSize);
        ThrowHelper.ThrowIfNegativeOrZero(maxSize);

        _pool = new ObjectPool<List<T>>(static () => [], null, static list => list.Clear(), initialSize, maxSize);
        _disposed = false;
    }

    public List<T> Rent()
    {
        ThrowHelper.ThrowIfDisposed(_disposed, this);

        return _pool.Rent();
    }

    public void Return(List<T> list)
    {
        ThrowHelper.ThrowIfDisposed(_disposed, this);

        _pool.Return(list);
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
