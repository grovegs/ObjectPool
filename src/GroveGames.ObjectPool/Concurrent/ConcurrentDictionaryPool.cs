namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentDictionaryPool<TKey, TValue> : IDictionaryPool<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentObjectPool<Dictionary<TKey, TValue>> _pool;
    private volatile bool _disposed;

    public int Count => _disposed ? throw new ObjectDisposedException(nameof(ConcurrentDictionaryPool<TKey, TValue>)) : _pool.Count;
    public int MaxSize => _disposed ? throw new ObjectDisposedException(nameof(ConcurrentDictionaryPool<TKey, TValue>)) : _pool.MaxSize;

    public ConcurrentDictionaryPool(int capacity, int maxSize, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _pool = new ConcurrentObjectPool<Dictionary<TKey, TValue>>(() => new Dictionary<TKey, TValue>(capacity, comparer), static dict => dict.Clear(), maxSize);
        _disposed = false;
    }

    public Dictionary<TKey, TValue> Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _pool.Rent();
    }

    public void Return(Dictionary<TKey, TValue> dictionary)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _pool.Return(dictionary);
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
