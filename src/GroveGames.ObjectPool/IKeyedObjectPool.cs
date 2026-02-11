using System;

namespace GroveGames.ObjectPool;

public interface IKeyedObjectPool<TKey, TValue> : IDisposable
    where TKey : notnull
    where TValue : class
{
    public int Count(TKey key);
    public int MaxSize(TKey key);
    public TValue Rent(TKey key);
    public void Return(TKey key, TValue item);
    public void Warm(TKey key);
    public void Warm();
    public void Clear(TKey key);
    public void Clear();
}
