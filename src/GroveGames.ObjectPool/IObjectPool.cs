using System;

namespace GroveGames.ObjectPool;

public interface IObjectPool<T> : IDisposable where T : class
{
    int Count { get; }
    int MaxSize { get; }
    T Rent();
    void Return(T item);
    void Clear();
}