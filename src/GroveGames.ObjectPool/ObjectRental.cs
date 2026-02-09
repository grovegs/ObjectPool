namespace GroveGames.ObjectPool;

public readonly ref struct ObjectRental<T>(IObjectPool<T> pool, T item) where T : class
{
    private readonly IObjectPool<T> _pool = pool;
    private readonly T _item = item;

    public T Value => _item;

    public void Dispose()
    {
        _pool.Return(_item);
    }
}
