using System;

namespace GroveGames.ObjectPool;

public interface IKeyedObjectPool<TKey, TValue> : IDisposable
{
    int PoolCount { get; }
    void Warm(TKey key);
    TValue Rent(TKey key);
    void Return(TKey key, TValue item);
    void Clear();
    void ClearKey(TKey key);
    bool ContainsKey(TKey key);
}
