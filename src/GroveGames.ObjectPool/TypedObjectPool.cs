using System;

namespace GroveGames.ObjectPool;

public sealed class TypedObjectPool<TBase, TDerived> : IObjectPool<TBase> where TDerived : class, TBase where TBase : class
{
    private readonly ObjectPool<TDerived> _pool;
    private bool _disposed;

    public int Count
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, typeof(TypedObjectPool<TBase, TDerived>));
            return _pool.Count;
        }
    }

    public int MaxSize
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, typeof(TypedObjectPool<TBase, TDerived>));
            return _pool.MaxSize;
        }
    }

    public TypedObjectPool(Func<TDerived> factory, Action<TDerived>? onRent, Action<TDerived>? onReturn, int initialSize, int maxSize)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, maxSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _pool = new ObjectPool<TDerived>(factory, onRent, onReturn, initialSize, maxSize);
        _disposed = false;
    }

    public TBase Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _pool.Rent();
    }

    public void Return(TBase item)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var derivedItem = (TDerived)item;
        _pool.Return(derivedItem);
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
