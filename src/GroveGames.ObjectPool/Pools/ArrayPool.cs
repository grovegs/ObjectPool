namespace GroveGames.ObjectPool.Pools;

public sealed class ArrayPool<T> : IArrayPool<T>
{
    private readonly ObjectPool<T[]> _objectPool;

    public ArrayPool(int size)
    {
        _objectPool = new ObjectPool<T[]>(size, new ArrayPooledObjectStrategy<T>(size));
    }

    public T[] Get(int size)
    {
        var array = _objectPool.Get();

        if (array.Length < size)
        {
            Array.Resize(ref array, size);
        }

        return array;
    }

    public IDisposable Get(int size, out T[] array)
    {
        array = Get(size);
        return new DisposablePooledObject<T[]>(Return, array);
    }

    public void Return(T[] array)
    {
        _objectPool.Return(array);
    }

    public void Dispose()
    {
        _objectPool.Dispose();
    }
}
