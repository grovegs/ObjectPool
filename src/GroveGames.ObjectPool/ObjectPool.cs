namespace GroveGames.ObjectPool;

public sealed class ObjectPool<T> : IObjectPool<T> where T : class
{
    private readonly Stack<T> _pool;
    private readonly IPooledObjectStrategy<T> _pooledObjectStrategy;

    public ObjectPool(int size, IPooledObjectStrategy<T> pooledObjectStrategy)
    {
        _pool = new Stack<T>(size);
        _pooledObjectStrategy = pooledObjectStrategy;
    }

    private T GetOrCreate()
    {
        if (!_pool.TryPop(out var pooledObject))
        {
            pooledObject = _pooledObjectStrategy.Create();
        }

        return pooledObject;
    }

    public T Get()
    {
        var pooledObject = GetOrCreate();
        _pooledObjectStrategy.Get(pooledObject);
        return pooledObject;
    }

    public IDisposable Get(out T pooledObject)
    {
        pooledObject = Get();
        return new DisposablePooledObject<T>(Return, pooledObject);
    }

    public void Return(T pooledObject)
    {
        _pooledObjectStrategy.Return(pooledObject);
        _pool.Push(pooledObject);
    }

    public void Dispose()
    {
        foreach (var item in _pool)
        {
            if (item is not IDisposable disposable)
            {
                continue;
            }

            disposable.Dispose();
        }

        _pool.Clear();
    }
}
