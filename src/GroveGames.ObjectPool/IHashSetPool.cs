namespace GroveGames.ObjectPool;

public interface IHashSetPool<T> : IDisposable
{
    int Count { get; }
    int MaxSize { get; }
    HashSet<T> Rent();
    void Return(HashSet<T> hashSet);
    void Clear();
}