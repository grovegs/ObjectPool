namespace GroveGames.ObjectPool;

public sealed class HashSetPool<T> : IHashSetPool<T> where T : notnull
{
    private readonly ObjectPool<HashSet<T>> _pool;
    private bool _disposed;

    public int Count => _disposed ? throw new ObjectDisposedException(nameof(HashSetPool<T>)) : _pool.Count;
    public int MaxSize => _disposed ? throw new ObjectDisposedException(nameof(HashSetPool<T>)) : _pool.MaxSize;

    public HashSetPool(int initialSize, int maxSize, IEqualityComparer<T>? comparer = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, maxSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _pool = new ObjectPool<HashSet<T>>(() => new HashSet<T>(comparer), null, static hashSet => hashSet.Clear(), initialSize, maxSize);
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