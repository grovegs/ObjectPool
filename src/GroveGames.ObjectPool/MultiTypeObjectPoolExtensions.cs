namespace GroveGames.ObjectPool;

public static class MultiTypeObjectPoolExtensions
{
    public static MultiTypeObjectRental<TBase, TDerived> Rent<TBase, TDerived>(this IMultiTypeObjectPool<TBase> pool, out TDerived item) where TBase : class where TDerived : class, TBase
    {
        item = (TDerived)pool.Rent<TDerived>();
        return new MultiTypeObjectRental<TBase, TDerived>(pool, item);
    }
}