using System;

namespace GroveGames.ObjectPool;

public interface IIndexedObjectPool<TValue> : IDisposable where TValue : class
{
    public int Count(int index);
    public int MaxSize(int index);
    public TValue Rent(int index);
    public void Return(int index, TValue item);
    public void Warm(int index);
    public void Warm();
    public void Clear(int index);
    public void Clear();
}
