using System;
using System.Threading;

namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentTypedObjectPool<TBase, TDerived> : IObjectPool<TBase> where TDerived : class, TBase where TBase : class
{
    private readonly ConcurrentObjectPool<TDerived> _pool;
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

    public ConcurrentTypedObjectPool(Func<TDerived> factory, Action<TDerived>? onRent, Action<TDerived>? onReturn, int initialSize, int maxSize)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, maxSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _pool = new ConcurrentObjectPool<TDerived>(factory, onRent, onReturn, initialSize, maxSize);
        _disposed = 0;
    }

    public TBase Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        return _pool.Rent();
    }

    public void Return(TBase item)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        var derivedItem = (TDerived)item;
        _pool.Return(derivedItem);
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
