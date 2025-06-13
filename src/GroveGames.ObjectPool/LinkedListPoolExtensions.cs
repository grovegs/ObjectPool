namespace GroveGames.ObjectPool;

public static class LinkedListPoolExtensions
{
    public static LinkedListRental<T> Rent<T>(this ILinkedListPool<T> pool, out LinkedList<T> linkedList)
    {
        linkedList = pool.Rent();
        return new LinkedListRental<T>(pool, linkedList);
    }
}
