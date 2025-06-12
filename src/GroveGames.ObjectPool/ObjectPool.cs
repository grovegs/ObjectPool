namespace GroveGames.ObjectPool;

public sealed class ObjectPool<T> : IObjectPool<T> where T : class
{
    private readonly Queue<T> _items;
    private readonly Func<T> _factory;
    private readonly Action<T>? _onReturn;
    private readonly int _maxSize;
    private bool _disposed;

    public int Count => _items.Count;
    public int MaxSize => _maxSize;

    public ObjectPool(Func<T> factory, Action<T>? onReturn, int maxSize, int initialSize, bool prewarmPool)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);

        _items = new Queue<T>(maxSize);
        _factory = factory;
        _onReturn = onReturn;
        _maxSize = maxSize;
        _disposed = false;

        if (prewarmPool && initialSize > 0)
        {
            PrewarmPool(initialSize);
        }
    }

    public T Rent()
    {
        return _items.Count > 0 ? _items.Dequeue() : _factory();
    }

    public void Return(T item)
    {
        if (_disposed)
        {
            return;
        }

        _onReturn?.Invoke(item);

        if (_items.Count < _maxSize)
        {
            _items.Enqueue(item);
        }
    }

    private void PrewarmPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var item = _factory();
            _onReturn?.Invoke(item);
            _items.Enqueue(item);
        }
    }

    public void Clear()
    {
        _items.Clear();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        while (_items.Count > 0)
        {
            T item = _items.Dequeue();

            if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
