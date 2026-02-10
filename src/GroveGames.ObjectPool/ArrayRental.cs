namespace GroveGames.ObjectPool;

public readonly ref struct ArrayRental<T>(IArrayPool<T> pool, T[] array) where T : notnull
{
    private readonly IArrayPool<T> _pool = pool;
    private readonly T[] _array = array;

    public T[] Value => _array;

    public void Dispose()
    {
        _pool.Return(_array);
    }
}
