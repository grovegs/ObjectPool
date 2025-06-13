namespace GroveGames.ObjectPool;

public static class HashSetPoolExtensions
{
    public static HashSetRental<T> Rent<T>(this IHashSetPool<T> pool, out HashSet<T> hashSet)
    {
        hashSet = pool.Rent();
        return new HashSetRental<T>(pool, hashSet);
    }
}
