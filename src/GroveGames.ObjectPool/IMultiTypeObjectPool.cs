namespace GroveGames.ObjectPool;

public interface IMultiTypeObjectPool<TBase> : IDisposable where TBase : class
{
    int Count<TDerived>() where TDerived : class, TBase;
    int MaxSize<TDerived>() where TDerived : class, TBase;
    TBase Rent<TDerived>() where TDerived : class, TBase;
    void Return<TDerived>(TDerived item) where TDerived : class, TBase;
    void Clear();
}
