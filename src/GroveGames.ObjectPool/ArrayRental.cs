namespace GroveGames.ObjectPool;

public readonly ref struct ArrayRental<T>(IArrayPool<T> pool, T[] array, bool clearArray)
{
    private readonly IArrayPool<T> _pool = pool;
    private readonly T[] _array = array;
    private readonly bool _clearArray = clearArray;

    public T[] Value => _array;

    public void Dispose()
    {
        _pool.Return(_array, _clearArray);
    }
}
