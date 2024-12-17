namespace GroveGames.ObjectPool;

public interface IObjectPool<T> : IDisposable where T : class
{
    T Get();
    IDisposable Get(out T pooledObject);
    void Return(T pooledObject);
}