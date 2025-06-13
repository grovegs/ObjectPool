namespace GroveGames.ObjectPool;

public static class DictionaryPoolExtensions
{
    public static DictionaryRental<TKey, TValue> Rent<TKey, TValue>(this IDictionaryPool<TKey, TValue> pool, out Dictionary<TKey, TValue> dictionary) where TKey : notnull
    {
        dictionary = pool.Rent();
        return new DictionaryRental<TKey, TValue>(pool, dictionary);
    }
}
