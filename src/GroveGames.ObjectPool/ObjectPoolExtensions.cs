namespace GroveGames.ObjectPool;

public static class ObjectPoolExtensions
{
    public static ObjectRental<T> Rent<T>(this IObjectPool<T> pool, out T item) where T : class
    {
        item = pool.Rent();
        return new ObjectRental<T>(pool, item);
    }
}
