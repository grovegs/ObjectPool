namespace GroveGames.ObjectPool;

public interface IPooledObjectStrategy<T>
{
    T Create();
    void Get(T pooledObject);
    void Return(T pooledObject);
}
