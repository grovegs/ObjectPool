namespace GroveGames.ObjectPool.Pools;

public sealed class ArrayPooledObjectStrategy<T> : IPooledObjectStrategy<T[]>
{
    private readonly int _size;

    public ArrayPooledObjectStrategy(int size)
    {
        _size = size;
    }

    public T[] Create()
    {
        return new T[_size];
    }

    public void Get(T[] array)
    {
    }

    public void Return(T[] array)
    {
        Array.Clear(array, 0, array.Length);
    }
}
