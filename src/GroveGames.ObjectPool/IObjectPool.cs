using System;

namespace GroveGames.ObjectPool;

public interface IObjectPool<T> : IDisposable where T : class
{
    public int Count { get; }
    public int MaxSize { get; }
    public T Rent();
    public void Return(T item);
    public void Clear();
    public void Warm();
}
