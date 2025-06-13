namespace GroveGames.ObjectPool;

public static class ListPoolExtensions
{
    public static ListRental<T> Rent<T>(this IListPool<T> pool, out List<T> list)
    {
        list = pool.Rent();
        return new ListRental<T>(pool, list);
    }
}
