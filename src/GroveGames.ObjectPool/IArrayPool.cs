namespace GroveGames.ObjectPool;

public interface IArrayPool<T> : IDisposable
{
    T[] Rent(int size);
    void Return(T[] array, bool clearArray = false);
    int Count(int size);
    int MaxSize(int size);
    void Clear();
}
