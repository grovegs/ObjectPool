using System.Collections.Concurrent;

namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentObjectPool<T> : IObjectPool<T> where T : class
{
    private readonly ConcurrentQueue<T> _items;
    private readonly Func<T> _factory;
    private readonly Action<T>? _onRent;
    private readonly Action<T>? _onReturn;
    private readonly int _initialSize;
    private readonly int _maxSize;
    private volatile int _disposed;

    public int Count => _disposed == 1
        ? throw new ObjectDisposedException(nameof(ConcurrentObjectPool<T>))
        : _items.Count;

    public int MaxSize => _disposed == 1
        ? throw new ObjectDisposedException(nameof(ConcurrentObjectPool<T>))
        : _maxSize;

    public ConcurrentObjectPool(Func<T> factory, Action<T>? onRent, Action<T>? onReturn, int initialSize, int maxSize)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, maxSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _items = new ConcurrentQueue<T>();
        _factory = factory;
        _onRent = onRent;
        _onReturn = onReturn;
        _initialSize = initialSize;
        _maxSize = maxSize;
        _disposed = 0;
    }

    public T Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        if (_items.TryDequeue(out var pooledItem))
        {
            _onRent?.Invoke(pooledItem);
            return pooledItem;
        }

        var item = _factory();
        _onRent?.Invoke(item);
        return item;
    }

    public void Return(T item)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        _onReturn?.Invoke(item);

        if (_items.Count < _maxSize)
        {
            _items.Enqueue(item);
        }
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        _items.Clear();
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