using System;
using System.Collections.Concurrent;
using System.Threading;

namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentObjectPool<T> : IObjectPool<T> where T : class
{
    private readonly ConcurrentQueue<T> _items;
    private readonly Func<T> _factory;
    private readonly Action<T>? _onRent;
    private readonly Action<T>? _onReturn;
    private readonly int _initialSize;
    private readonly int _maxSize;
    private volatile int _count;
    private volatile int _disposed;

    public int Count
    {
        get
        {
            ThrowHelper.ThrowIfDisposed(_disposed == 1, this);
            return _count;
        }
    }

    public int MaxSize
    {
        get
        {
            ThrowHelper.ThrowIfDisposed(_disposed == 1, this);
            return _maxSize;
        }
    }

    public ConcurrentObjectPool(Func<T> factory, Action<T>? onRent, Action<T>? onReturn, int initialSize, int maxSize)
    {
        ThrowHelper.ThrowIfNull(factory);
        ThrowHelper.ThrowIfNegative(initialSize);
        ThrowHelper.ThrowIfGreaterThan(initialSize, maxSize);
        ThrowHelper.ThrowIfNegativeOrZero(maxSize);

        _items = new ConcurrentQueue<T>();
        _factory = factory;
        _onRent = onRent;
        _onReturn = onReturn;
        _initialSize = initialSize;
        _maxSize = maxSize;
        _count = 0;
        _disposed = 0;
    }

    public T Rent()
    {
        ThrowHelper.ThrowIfDisposed(_disposed == 1, this);

        if (_items.TryDequeue(out var pooledItem))
        {
            Interlocked.Decrement(ref _count);
            _onRent?.Invoke(pooledItem);
            return pooledItem;
        }

        var item = _factory();
        _onRent?.Invoke(item);
        return item;
    }

    public void Return(T item)
    {
        ThrowHelper.ThrowIfDisposed(_disposed == 1, this);

        _onReturn?.Invoke(item);

        if (Interlocked.Increment(ref _count) <= _maxSize)
        {
            _items.Enqueue(item);
        }
        else
        {
            Interlocked.Decrement(ref _count);
        }
    }

    public void Clear()
    {
        ThrowHelper.ThrowIfDisposed(_disposed == 1, this);

        _items.Clear();
        Interlocked.Exchange(ref _count, 0);
    }

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
        {
            return;
        }

        while (_items.TryDequeue(out var item))
        {
            if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _items.Clear();
    }
}