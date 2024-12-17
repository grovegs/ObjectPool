namespace GroveGames.ObjectPool;

public interface IPooledObjectStrategy<T> where T : class
{
    T Create();
    void Get(T pooledObject);
    void Return(T pooledObject);
}
