namespace GroveGames.ObjectPool;

public sealed class TypedObjectPool<TBase, TDerived> : IObjectPool<TBase> where TDerived : class, TBase where TBase : class
{
    private readonly Queue<TDerived> _items;
    private readonly Func<TDerived> _factory;
    private readonly Action<TDerived>? _onReturn;
    private readonly int _maxSize;
    private bool _disposed;

    public int Count => _disposed ? throw new ObjectDisposedException(nameof(TypedObjectPool<TBase, TDerived>)) : _items.Count;
    public int MaxSize => _disposed ? throw new ObjectDisposedException(nameof(TypedObjectPool<TBase, TDerived>)) : _maxSize;

    public TypedObjectPool(Func<TDerived> factory, Action<TDerived>? onReturn, int maxSize)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _items = new Queue<TDerived>(maxSize);
        _factory = factory;
        _onReturn = onReturn;
        _maxSize = maxSize;
        _disposed = false;
    }

    public TBase Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _items.TryDequeue(out var item) ? item : _factory();
    }

    public void Return(TBase item)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var derivedItem = (TDerived)item;
        _onReturn?.Invoke(derivedItem);

        if (_items.Count < _maxSize)
        {
            _items.Enqueue(derivedItem);
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
