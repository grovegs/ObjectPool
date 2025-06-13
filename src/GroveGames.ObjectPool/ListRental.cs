namespace GroveGames.ObjectPool;

public readonly ref struct ListRental<T>(IListPool<T> pool, List<T> list)
{
    private readonly IListPool<T> _pool = pool;
    private readonly List<T> _list = list;

    public List<T> Value => _list;

    public void Dispose()
    {
        _pool.Return(_list);
    }
}
