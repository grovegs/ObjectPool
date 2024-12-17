using System;

namespace GroveGames.ObjectPool;

public sealed class PooledObjectStrategy<T> : IPooledObjectStrategy<T> where T : class
{
    private readonly Func<T> _createObject;
    private readonly Action<T> _returnObject;

    public PooledObjectStrategy(Func<T> createObject, Action<T> returnObject)
    {
        _createObject = createObject;
        _returnObject = returnObject;
    }

    public T Create()
    {
        return _createObject();
    }

    public void Return(T pooledObject)
    {
        _returnObject(pooledObject);
    }
}
