using System;

namespace GroveGames.ObjectPool;

public interface IMultiTypeObjectPool<TBase> : IDisposable where TBase : class
{
    public int Count<TDerived>() where TDerived : class, TBase;
    public int MaxSize<TDerived>() where TDerived : class, TBase;
    public TBase Rent<TDerived>() where TDerived : class, TBase;
    public void Return<TDerived>(TDerived item) where TDerived : class, TBase;
    public void Warm<TDerived>() where TDerived : class, TBase;
    public void Warm();
    public void Clear<TDerived>() where TDerived : class, TBase;
    public void Clear();
}
