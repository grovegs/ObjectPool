namespace GroveGames.ObjectPool;

public sealed class TypedObjectPool<TBase, TDerived> : IObjectPool<TBase> where TDerived : class, TBase where TBase : class
{
    private readonly ObjectPool<TDerived> _pool;
    private bool _disposed;

    public int Count => _disposed ? throw new ObjectDisposedException(nameof(TypedObjectPool<TBase, TDerived>)) : _pool.Count;
    public int MaxSize => _disposed ? throw new ObjectDisposedException(nameof(TypedObjectPool<TBase, TDerived>)) : _pool.MaxSize;

    public TypedObjectPool(Func<TDerived> factory, Action<TDerived>? onReturn, int maxSize)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _pool = new ObjectPool<TDerived>(factory, onReturn, maxSize);
        _disposed = false;
    }

    public TBase Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _pool.Rent();
    }

    public void Return(TBase item)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var derivedItem = (TDerived)item;
        _pool.Return(derivedItem);
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _pool.Clear();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _pool.Dispose();
    }
}
