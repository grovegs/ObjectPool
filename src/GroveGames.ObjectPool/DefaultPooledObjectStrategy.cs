namespace GroveGames.ObjectPool;

public sealed class DefaultPooledObjectStrategy<T> : IPooledObjectStrategy<T> where T : class, new()
{
    public T Create()
    {
        return new T();
    }

    public void Return(T pooledObject)
    {
    }
}