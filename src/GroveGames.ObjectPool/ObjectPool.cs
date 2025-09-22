using System;
using System.Collections.Generic;

namespace GroveGames.ObjectPool;

public sealed class ObjectPool<T> : IObjectPool<T> where T : class
{
    private readonly Queue<T> _items;
    private readonly Func<T> _factory;
    private readonly Action<T>? _onRent;
    private readonly Action<T>? _onReturn;
    private readonly int _maxSize;
    private bool _disposed;

    public int Count
    {
        get
        {
            ThrowHelper.ThrowIfDisposed(_disposed, this);
            return _items.Count;
        }
    }

    public int MaxSize
    {
        get
        {
            ThrowHelper.ThrowIfDisposed(_disposed, this);
            return _maxSize;
        }
    }

    public ObjectPool(Func<T> factory, Action<T>? onRent, Action<T>? onReturn, int initialSize, int maxSize)
    {
        ThrowHelper.ThrowIfNull(factory);
        ThrowHelper.ThrowIfNegative(initialSize);
        ThrowHelper.ThrowIfGreaterThan(initialSize, maxSize);
        ThrowHelper.ThrowIfNegativeOrZero(maxSize);

        _items = new Queue<T>(initialSize);
        _factory = factory;
        _onRent = onRent;
        _onReturn = onReturn;
        _maxSize = maxSize;
        _disposed = false;
    }

    public T Rent()
    {
        ThrowHelper.ThrowIfDisposed(_disposed, this);

        var item = _items.TryDequeue(out var pooledItem) ? pooledItem : _factory();
        _onRent?.Invoke(item);
        return item;
    }

    public void Return(T item)
    {
        ThrowHelper.ThrowIfDisposed(_disposed, this);

        _onReturn?.Invoke(item);

        if (_items.Count < _maxSize)
        {
            _items.Enqueue(item);
        }
    }

    public void Clear()
    {
        ThrowHelper.ThrowIfDisposed(_disposed, this);

        _items.Clear();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        while (_items.TryDequeue(out var item))
        {
            if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}