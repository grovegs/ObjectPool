namespace GroveGames.ObjectPool;

public interface ILinkedListPool<T> : IDisposable
{
    int Count { get; }
    int MaxSize { get; }
    LinkedList<T> Rent();
    void Return(LinkedList<T> linkedList);
    void Clear();
}
