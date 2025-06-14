namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentLinkedListPool<T> : ILinkedListPool<T> where T : notnull
{
    private readonly ConcurrentObjectPool<LinkedList<T>> _pool;
    private volatile bool _disposed;

    public int Count => _disposed ? throw new ObjectDisposedException(nameof(ConcurrentLinkedListPool<T>)) : _pool.Count;
    public int MaxSize => _disposed ? throw new ObjectDisposedException(nameof(ConcurrentLinkedListPool<T>)) : _pool.MaxSize;

    public ConcurrentLinkedListPool(int initialSize, int maxSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, maxSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _pool = new ConcurrentObjectPool<LinkedList<T>>(static () => new LinkedList<T>(), null, static list => list.Clear(), initialSize, maxSize);
        _disposed = false;
    }

    public LinkedList<T> Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _pool.Rent();
    }

    public void Return(LinkedList<T> linkedList)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _pool.Return(linkedList);
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
