namespace GroveGames.ObjectPool;

public interface IStackPool<T> : IDisposable
{
    int Count { get; }
    int MaxSize { get; }
    Stack<T> Rent();
    void Return(Stack<T> stack);
    void Clear();
}
