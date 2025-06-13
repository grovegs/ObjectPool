using System.Collections.Concurrent;

namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentObjectPool<T> : IObjectPool<T> where T : class
{
    private readonly ConcurrentQueue<T> _items;
    private readonly Func<T> _factory;
    private readonly Action<T>? _onReturn;
    private readonly int _maxSize;
    private volatile int _count;
    private volatile bool _disposed;

    public int Count => _disposed ? throw new ObjectDisposedException(nameof(ConcurrentObjectPool<T>)) : _count;
    public int MaxSize => _disposed ? throw new ObjectDisposedException(nameof(ConcurrentObjectPool<T>)) : _maxSize;

    public ConcurrentObjectPool(Func<T> factory, Action<T>? onReturn, int maxSize)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _items = new ConcurrentQueue<T>();
        _factory = factory;
        _onReturn = onReturn;
        _maxSize = maxSize;
        _count = 0;
        _disposed = false;
    }

    public T Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_items.TryDequeue(out var item))
        {
            Interlocked.Decrement(ref _count);
            return item;
        }

        return _factory();
    }

    public void Return(T item)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _onReturn?.Invoke(item);
        var count = Interlocked.Increment(ref _count);

        if (count <= _maxSize)
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
        ObjectDisposedException.ThrowIf(_disposed, this);

        _items.Clear();
        Interlocked.Exchange(ref _count, 0);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        while (_items.TryDequeue(out T? item))
        {
            Interlocked.Decrement(ref _count);

            if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
