namespace GroveGames.ObjectPool.Pools;

public interface IArrayPool<T> : IObjectPool<T[]>
{
    T[] Get(int size);
    IDisposable Get(out T[] array, int size);
}
