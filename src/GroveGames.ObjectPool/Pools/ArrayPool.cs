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
        var array = Get();

        if (array.Length < size)
        {
            Array.Resize(ref array, size);
        }

        return array;
    }

    public T[] Get()
    {
        return _objectPool.Get();
    }

    public IDisposable Get(out T[] array, int size)
    {
        array = Get(size);
        return new DisposablePooledObject<T[]>(Return, array);
    }

    public IDisposable Get(out T[] array)
    {
        return _objectPool.Get(out array);
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
