namespace GroveGames.ObjectPool;

public sealed class DefaultObjectPool<T> : IObjectPool<T> where T : class, new()
{
    private readonly ObjectPool<T> _objectPool;

    public DefaultObjectPool(int size)
    {
        var strategy = new DefaultPooledObjectStrategy<T>();
        _objectPool = new ObjectPool<T>(size, strategy);
    }

    public T Get()
    {
        return _objectPool.Get();
    }

    public IDisposable Get(out T pooledObject)
    {
        return _objectPool.Get(out pooledObject);
    }

    public void Return(T pooledObject)
    {
        _objectPool.Return(pooledObject);
    }

    public void Dispose()
    {
        _objectPool.Dispose();
    }
}
