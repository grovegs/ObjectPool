namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentDictionaryPool<TKey, TValue> : IDictionaryPool<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentObjectPool<Dictionary<TKey, TValue>> _pool;
    private volatile int _disposed;

    public int Count => _disposed == 1 ? throw new ObjectDisposedException(nameof(ConcurrentDictionaryPool<TKey, TValue>)) : _pool.Count;
    public int MaxSize => _disposed == 1 ? throw new ObjectDisposedException(nameof(ConcurrentDictionaryPool<TKey, TValue>)) : _pool.MaxSize;

    public ConcurrentDictionaryPool(int initialSize, int maxSize, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, maxSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _pool = new ConcurrentObjectPool<Dictionary<TKey, TValue>>(
            () => new Dictionary<TKey, TValue>(comparer),
            null,
            static dictionary => dictionary.Clear(),
            initialSize,
            maxSize);
        _disposed = 0;
    }

    public Dictionary<TKey, TValue> Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        return _pool.Rent();
    }

    public void Return(Dictionary<TKey, TValue> dictionary)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        _pool.Return(dictionary);
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