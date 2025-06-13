namespace GroveGames.ObjectPool;

public interface IArrayPool<T> : IDisposable
{
    int Count(int size);
    int MaxSize(int size);
    T[] Rent(int size);
    void Return(T[] array, bool clearArray = false);
    void Clear();
}
