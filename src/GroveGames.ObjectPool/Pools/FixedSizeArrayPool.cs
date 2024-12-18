namespace GroveGames.ObjectPool.Pools;

public sealed class FixedSizeArrayPool<T> : IArrayPool<T>
{
    private readonly Dictionary<int, ObjectPool<T[]>> _objectPoolsBySizes;

    public FixedSizeArrayPool()
    {
        _objectPoolsBySizes = [];
    }

    public T[] Get(int size)
    {
        if (!_objectPoolsBySizes.TryGetValue(size, out var objectPool))
        {
            objectPool = new ObjectPool<T[]>(size, new ArrayPooledObjectStrategy<T>(size));
            _objectPoolsBySizes.Add(size, objectPool);
        }

        var array = objectPool.Get();
        return array;
    }

    public IDisposable Get(int size, out T[] array)
    {
        array = Get(size);
        return new DisposablePooledObject<T[]>(Return, array);
    }

    public void Return(T[] array)
    {
        var objectPool = _objectPoolsBySizes[array.Length];
        objectPool.Return(array);
    }

    public void Dispose()
    {
        foreach (var objectPool in _objectPoolsBySizes.Values)
        {
            objectPool.Dispose();
        }

        _objectPoolsBySizes.Clear();
    }
}
