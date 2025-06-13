namespace GroveGames.ObjectPool;

public interface IListPool<T> : IDisposable
{
    int Count { get; }
    int MaxSize { get; }
    List<T> Rent();
    void Return(List<T> list);
    void Clear();
}
