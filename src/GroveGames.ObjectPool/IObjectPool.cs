namespace GroveGames.ObjectPool;

public interface IObjectPool<T> : IDisposable where T : class
{
    T Rent();
    void Return(T item);
    int Count { get; }
    int MaxSize { get; }
    void Clear();
}