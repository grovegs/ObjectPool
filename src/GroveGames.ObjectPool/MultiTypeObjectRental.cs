namespace GroveGames.ObjectPool;

public readonly ref struct MultiTypeObjectRental<TBase, TDerived>(IMultiTypeObjectPool<TBase> pool, TDerived item) where TBase : class where TDerived : class, TBase
{
    private readonly IMultiTypeObjectPool<TBase> _pool = pool;
    private readonly TDerived _item = item;

    public TDerived Value => _item;

    public void Dispose()
    {
        _pool.Return(_item);
    }
}
