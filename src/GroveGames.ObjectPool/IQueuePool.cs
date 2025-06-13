namespace GroveGames.ObjectPool;

public interface IQueuePool<T> : IDisposable
{
    int Count { get; }
    int MaxSize { get; }
    Queue<T> Rent();
    void Return(Queue<T> queue);
    void Clear();
}
