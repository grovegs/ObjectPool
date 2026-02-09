using System;

namespace GroveGames.ObjectPool;

public interface IArrayPool<T> : IDisposable where T : notnull
{
    public int Count(int size);
    public int MaxSize(int size);
    public T[] Rent(int size);
    public void Return(T[] array);
    public void Clear();
}
