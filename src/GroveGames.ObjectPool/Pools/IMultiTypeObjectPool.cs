namespace GroveGames.ObjectPool.Pools;

public interface IMultiTypeObjectPool<TBase> : IDisposable where TBase : class
{
    void AddPooledObjectStrategy<TDerived>(IPooledObjectStrategy<TBase> pooledObjectStrategy) where TDerived : class, TBase;
    void RemovePooledObjectStrategy<TDerived>() where TDerived : class, TBase;
    TDerived Get<TDerived>() where TDerived : class, TBase;
    IDisposable Get<TDerived>(out TDerived pooledObject) where TDerived : class, TBase;
    void Return<TDerived>(TDerived pooledObject) where TDerived : class, TBase;
}
