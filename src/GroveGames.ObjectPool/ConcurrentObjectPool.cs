using System.Collections.Concurrent;

namespace GroveGames.ObjectPool;

public sealed class ConcurrentObjectPool<T> : IObjectPool<T> where T : class
{
    private readonly ConcurrentQueue<T> _items;
    private readonly Func<T> _factory;
    private readonly Action<T>? _onReturn;
    private readonly int _maxSize;
    private volatile int _count;
    private volatile bool _disposed;

    public int Count => _count;
    public int MaxSize => _maxSize;

    public ConcurrentObjectPool(Func<T> factory, Action<T>? onReturn, int maxSize, int initialSize, bool prewarm)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);

        _items = new ConcurrentQueue<T>();
        _factory = factory;
        _onReturn = onReturn;
        _maxSize = maxSize;
        _count = 0;
        _disposed = false;

        if (prewarm && initialSize > 0)
        {
            Prewarm(initialSize);
        }
    }

    public T Rent()
    {
        if (_items.TryDequeue(out T? item))
        {
            Interlocked.Decrement(ref _count);
            return item;
        }

        return _factory();
    }

    public void Return(T item)
    {
        if (_disposed)
        {
            return;
        }

        _onReturn?.Invoke(item);

        int count = Interlocked.Increment(ref _count);

        if (count <= _maxSize)
        {
            _items.Enqueue(item);
        }
        else
        {
            Interlocked.Decrement(ref _count);
        }
    }

    private void Prewarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var item = _factory();
            _onReturn?.Invoke(item);
            _items.Enqueue(item);
            Interlocked.Increment(ref _count);
        }
    }

    public void Clear()
    {
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
