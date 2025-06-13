namespace GroveGames.ObjectPool;

public interface IDictionaryPool<TKey, TValue> : IDisposable where TKey : notnull
{
    int Count { get; }
    int MaxSize { get; }
    Dictionary<TKey, TValue> Rent();
    void Return(Dictionary<TKey, TValue> dictionary);
    void Clear();
}