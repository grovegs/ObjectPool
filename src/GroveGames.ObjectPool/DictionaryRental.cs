namespace GroveGames.ObjectPool;

public readonly ref struct DictionaryRental<TKey, TValue>(IDictionaryPool<TKey, TValue> pool, Dictionary<TKey, TValue> dictionary) where TKey : notnull
{
    private readonly IDictionaryPool<TKey, TValue> _pool = pool;
    private readonly Dictionary<TKey, TValue> _dictionary = dictionary;

    public Dictionary<TKey, TValue> Value => _dictionary;

    public void Dispose()
    {
        _pool.Return(_dictionary);
    }
}
