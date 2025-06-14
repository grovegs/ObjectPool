namespace GroveGames.ObjectPool;

public sealed class ObjectPool<T> : IObjectPool<T> where T : class
{
    private readonly Queue<T> _items;
    private readonly Func<T> _factory;
    private readonly Action<T>? _onReturn;
    private readonly int _maxSize;
    private bool _disposed;

    public int Count => _disposed ? throw new ObjectDisposedException(nameof(ObjectPool<T>)) : _items.Count;
    public int MaxSize => _disposed ? throw new ObjectDisposedException(nameof(ObjectPool<T>)) : _maxSize;

    public ObjectPool(Func<T> factory, Action<T>? onReturn, int maxSize)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _items = new Queue<T>(maxSize);
        _factory = factory;
        _onReturn = onReturn;
        _maxSize = maxSize;
        _disposed = false;
    }

    public T Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _items.TryDequeue(out var item) ? item : _factory();
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
