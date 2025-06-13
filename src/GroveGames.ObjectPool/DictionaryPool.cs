namespace GroveGames.ObjectPool;

public sealed class DictionaryPool<TKey, TValue> : IDictionaryPool<TKey, TValue> where TKey : notnull
{
    private readonly ObjectPool<Dictionary<TKey, TValue>> _pool;
    private bool _disposed;

    public int Count => _disposed ? throw new ObjectDisposedException(nameof(DictionaryPool<TKey, TValue>)) : _pool.Count;
    public int MaxSize => _disposed ? throw new ObjectDisposedException(nameof(DictionaryPool<TKey, TValue>)) : _pool.MaxSize;

    public DictionaryPool(int maxSize, int capacity, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);

        _pool = new ObjectPool<Dictionary<TKey, TValue>>(() => new Dictionary<TKey, TValue>(capacity, comparer), static dictionary => dictionary.Clear(), maxSize);
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