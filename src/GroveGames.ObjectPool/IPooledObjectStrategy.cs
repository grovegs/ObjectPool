namespace GroveGames.ObjectPool;

public interface IPooledObjectStrategy<T> where T : class
{
    T Create();
    void Return(T pooledObject);
}
