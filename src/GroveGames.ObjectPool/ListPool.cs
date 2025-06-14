namespace GroveGames.ObjectPool;

public sealed class ListPool<T> : IListPool<T> where T : notnull
{
    private readonly ObjectPool<List<T>> _pool;
    private bool _disposed;

    public int Count => _disposed ? throw new ObjectDisposedException(nameof(ListPool<T>)) : _pool.Count;
    public int MaxSize => _disposed ? throw new ObjectDisposedException(nameof(ListPool<T>)) : _pool.MaxSize;

    public ListPool(int maxSize, int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);

        _pool = new ObjectPool<List<T>>(() => new List<T>(capacity), static list => list.Clear(), maxSize);
    }

    public List<T> Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _pool.Rent();
    }

    public void Return(List<T> list)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _pool.Return(list);
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
