namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentHashSetPool<T> : IHashSetPool<T> where T : notnull
{
    private readonly ConcurrentObjectPool<HashSet<T>> _pool;
    private volatile bool _disposed;

    public int Count => _disposed ? throw new ObjectDisposedException(nameof(ConcurrentHashSetPool<T>)) : _pool.Count;
    public int MaxSize => _disposed ? throw new ObjectDisposedException(nameof(ConcurrentHashSetPool<T>)) : _pool.MaxSize;

    public ConcurrentHashSetPool(int initialSize, int maxSize, IEqualityComparer<T>? comparer = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, maxSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _pool = new ConcurrentObjectPool<HashSet<T>>(() => new HashSet<T>(comparer), null, static set => set.Clear(), initialSize, maxSize);
        _disposed = false;
    }

    public HashSet<T> Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _pool.Rent();
    }

    public void Return(HashSet<T> hashSet)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _pool.Return(hashSet);
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
