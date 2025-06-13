namespace GroveGames.ObjectPool;

public readonly ref struct HashSetRental<T>(IHashSetPool<T> pool, HashSet<T> hashSet)
{
    private readonly IHashSetPool<T> _pool = pool;
    private readonly HashSet<T> _hashSet = hashSet;

    public HashSet<T> Value => _hashSet;

    public void Dispose()
    {
        _pool.Return(_hashSet);
    }
}
