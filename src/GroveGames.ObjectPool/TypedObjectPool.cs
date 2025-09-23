using System;

namespace GroveGames.ObjectPool;

public sealed class TypedObjectPool<TBase, TDerived> : IObjectPool<TBase> where TDerived : class, TBase where TBase : class
{
    private readonly ObjectPool<TDerived> _pool;
    private bool _disposed;

    public int Count => _disposed ? throw new ObjectDisposedException(nameof(TypedObjectPool<TBase, TDerived>)) : _pool.Count;
    public int MaxSize => _disposed ? throw new ObjectDisposedException(nameof(TypedObjectPool<TBase, TDerived>)) : _pool.MaxSize;

    public TypedObjectPool(Func<TDerived> factory, Action<TDerived>? onRent, Action<TDerived>? onReturn, int initialSize, int maxSize)
    {
        ThrowHelper.ThrowIfNull(factory);
        ThrowHelper.ThrowIfNegative(initialSize);
        ThrowHelper.ThrowIfGreaterThan(initialSize, maxSize);
        ThrowHelper.ThrowIfNegativeOrZero(maxSize);

        _pool = new ObjectPool<TDerived>(factory, onRent, onReturn, initialSize, maxSize);
        _disposed = false;
    }

    public TBase Rent()
    {
        ThrowHelper.ThrowIfDisposed(_disposed, this);

        return _pool.Rent();
    }

    public void Return(TBase item)
    {
        ThrowHelper.ThrowIfDisposed(_disposed, this);

        var derivedItem = (TDerived)item;
        _pool.Return(derivedItem);
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
