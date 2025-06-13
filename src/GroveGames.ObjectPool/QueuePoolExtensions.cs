namespace GroveGames.ObjectPool;

public static class QueuePoolExtensions
{
    public static QueueRental<T> Rent<T>(this IQueuePool<T> pool, out Queue<T> queue)
    {
        queue = pool.Rent();
        return new QueueRental<T>(pool, queue);
    }
}
