namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentHashSetPool<T> : IHashSetPool<T> where T : notnull
{
    private readonly ConcurrentObjectPool<HashSet<T>> _pool;
    private volatile int _disposed;

    public int Count => _disposed == 1 ? throw new ObjectDisposedException(nameof(ConcurrentHashSetPool<T>)) : _pool.Count;
    public int MaxSize => _disposed == 1 ? throw new ObjectDisposedException(nameof(ConcurrentHashSetPool<T>)) : _pool.MaxSize;

    public ConcurrentHashSetPool(int initialSize, int maxSize, IEqualityComparer<T>? comparer = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, maxSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _pool = new ConcurrentObjectPool<HashSet<T>>(
            () => new HashSet<T>(comparer),
            null,
            static set => set.Clear(),
            initialSize,
            maxSize);
        _disposed = 0;
    }

    public HashSet<T> Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        return _pool.Rent();
    }

    public void Return(HashSet<T> hashSet)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        _pool.Return(hashSet);
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