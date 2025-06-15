namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentLinkedListPool<T> : ILinkedListPool<T> where T : notnull
{
    private readonly ConcurrentObjectPool<LinkedList<T>> _pool;
    private volatile int _disposed;

    public int Count => _disposed == 1 ? throw new ObjectDisposedException(nameof(ConcurrentLinkedListPool<T>)) : _pool.Count;
    public int MaxSize => _disposed == 1 ? throw new ObjectDisposedException(nameof(ConcurrentLinkedListPool<T>)) : _pool.MaxSize;

    public ConcurrentLinkedListPool(int initialSize, int maxSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, maxSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _pool = new ConcurrentObjectPool<LinkedList<T>>(
            static () => new LinkedList<T>(),
            null,
            static list => list.Clear(),
            initialSize,
            maxSize);
        _disposed = 0;
    }

    public LinkedList<T> Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        return _pool.Rent();
    }

    public void Return(LinkedList<T> linkedList)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        _pool.Return(linkedList);
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        _pool.Clear();
    }

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
        {
            return;
        }

        _pool.Dispose();
    }
}