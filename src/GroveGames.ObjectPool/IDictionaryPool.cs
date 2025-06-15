namespace GroveGames.ObjectPool;

public interface IDictionaryPool<TKey, TValue> : IObjectPool<Dictionary<TKey, TValue>> where TKey : notnull
{
}