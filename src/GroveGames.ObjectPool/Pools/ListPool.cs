namespace GroveGames.ObjectPool.Pools;

public sealed class ListPool<T> : IObjectPool<List<T>>
{
    private readonly ObjectPool<List<T>> _objectPool;

    public ListPool(int size)
    {
        _objectPool = new ObjectPool<List<T>>(size, new ListPooledObjectStrategy<T>(size));
    }

    public List<T> Get()
    {
        return _objectPool.Get();
    }

    public IDisposable Get(out List<T> list)
    {
        return _objectPool.Get(out list);
    }

    public void Return(List<T> list)
    {
        _objectPool.Return(list);
    }

    public void Dispose()
    {
        _objectPool.Dispose();
    }
}
