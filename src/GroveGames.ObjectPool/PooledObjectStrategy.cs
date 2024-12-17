namespace GroveGames.ObjectPool;

public sealed class PooledObjectStrategy<T> : IPooledObjectStrategy<T>
{
    private readonly Func<T> _create;
    private readonly Action<T> _onGet;
    private readonly Action<T> _onReturn;

    public PooledObjectStrategy(Func<T> create, Action<T> onGet, Action<T> onReturn)
    {
        _create = create;
        _onGet = onGet;
        _onReturn = onReturn;
    }

    public T Create()
    {
        return _create();
    }

    public void Get(T pooledObject)
    {
        _onGet(pooledObject);
    }

    public void Return(T pooledObject)
    {
        _onReturn(pooledObject);
    }
}
