namespace GroveGames.ObjectPool.Pools;

public interface IArrayPool<T> : IDisposable
{
    T[] Get(int size);
    IDisposable Get(int size, out T[] array);
    void Return(T[] array);
}
