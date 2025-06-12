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

    public ObjectPool(Func<T> factory, Action<T>? onReturn, int maxSize, int initialSize, bool prewarm)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);

        _items = new Queue<T>(maxSize);
        _factory = factory;
        _onReturn = onReturn;
        _maxSize = maxSize;
        _disposed = false;

        if (prewarm && initialSize > 0)
        {
            Prewarm(initialSize);
        }
    }

    public T Rent()
    {
        return _items.TryDequeue(out T? item) ? item : _factory();
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

    private void Prewarm(int count)
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

        while (_items.TryDequeue(out T? item))
        {
            if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}