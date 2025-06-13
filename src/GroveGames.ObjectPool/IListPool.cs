namespace GroveGames.ObjectPool;

public interface IListPool<T> : IDisposable
{
    List<T> Rent();
    void Return(List<T> list);
    int Count { get; }
    int MaxSize { get; }
    void Clear();
}