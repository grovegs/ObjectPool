namespace GroveGames.ObjectPool;

public interface IObjectPool<T> : IDisposable
{
    T Get();
    IDisposable Get(out T pooledObject);
    void Return(T pooledObject);
}