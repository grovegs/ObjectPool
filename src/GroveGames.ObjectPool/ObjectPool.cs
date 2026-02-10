using System;
using System.Collections.Generic;

namespace GroveGames.ObjectPool;

public sealed class ObjectPool<T> : IObjectPool<T> where T : class
{
    private readonly Queue<T> _items;
    private readonly Func<T> _factory;
    private readonly Action<T>? _onRent;
    private readonly Action<T>? _onReturn;
    private readonly int _initialSize;
    private readonly int _maxSize;
    private bool _disposed;

    public int Count
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _items.Count;
        }
    }

    public int MaxSize
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _maxSize;
        }
    }

    public ObjectPool(Func<T> factory, Action<T>? onRent, Action<T>? onReturn, int initialSize, int maxSize)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, maxSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _items = new Queue<T>(initialSize);
        _factory = factory;
        _onRent = onRent;
        _onReturn = onReturn;
        _initialSize = initialSize;
        _maxSize = maxSize;
        _disposed = false;
    }

    public T Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var item = _items.TryDequeue(out var pooledItem) ? pooledItem : _factory();
        _onRent?.Invoke(item);
        return item;
    }

    public void Return(T item)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _onReturn?.Invoke(item);

        if (_items.Count < _maxSize)
        {
            _items.Enqueue(item);
        }
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _items.Clear();
    }

    public void Warm()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        for (int i = 0; i < _initialSize; i++)
        {
            _items.Enqueue(_factory());
        }
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
