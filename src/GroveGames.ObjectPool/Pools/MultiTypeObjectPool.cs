namespace GroveGames.ObjectPool.Pools;

public sealed class MultiTypeObjectPool<TBase> : IMultiTypeObjectPool<TBase> where TBase : class
{
    private readonly Dictionary<Type, ObjectPool<TBase>> _objectPoolsByTypes;
    private readonly Dictionary<Type, IPooledObjectStrategy<TBase>> _pooledObjectStrategiesByTypes;

    public MultiTypeObjectPool()
    {
        _objectPoolsByTypes = [];
        _pooledObjectStrategiesByTypes = [];
    }

    public void AddPooledObjectStrategy<TDerived>(IPooledObjectStrategy<TBase> pooledObjectStrategy) where TDerived : class, TBase
    {
        var type = typeof(TDerived);
        _pooledObjectStrategiesByTypes.Add(type, pooledObjectStrategy);
    }

    public void RemovePooledObjectStrategy<TDerived>() where TDerived : class, TBase
    {
        var type = typeof(TDerived);
        _pooledObjectStrategiesByTypes.Remove(type);
    }

    public TDerived Get<TDerived>() where TDerived : class, TBase
    {
        var type = typeof(TDerived);

        if (!_objectPoolsByTypes.TryGetValue(type, out var objectPool))
        {
            if (!_pooledObjectStrategiesByTypes.TryGetValue(type, out var pooledObjectStrategy))
            {
                throw new InvalidOperationException($"No pooling strategy registered for type {type}.");
            }

            objectPool = new ObjectPool<TBase>(0, pooledObjectStrategy);
            _objectPoolsByTypes.Add(type, objectPool);
        }

        return objectPool.Get() is not TDerived pooledObject
            ? throw new InvalidOperationException($"Failed to retrieve an object of type {typeof(TDerived)} from the pool.")
            : pooledObject;
    }

    public IDisposable Get<TDerived>(out TDerived pooledObject) where TDerived : class, TBase
    {
        pooledObject = Get<TDerived>();
        return new DisposablePooledObject<TDerived>(Return, pooledObject);
    }

    public void Return<TDerived>(TDerived pooledObject) where TDerived : class, TBase
    {
        var type = typeof(TDerived);
        var objectPool = _objectPoolsByTypes[type];
        objectPool.Return(pooledObject);
    }

    public void Dispose()
    {
        foreach (var objectPool in _objectPoolsByTypes.Values)
        {
            objectPool.Dispose();
        }

        _objectPoolsByTypes.Clear();
        _pooledObjectStrategiesByTypes.Clear();
    }
}