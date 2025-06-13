namespace GroveGames.ObjectPool;

public static class StackPoolExtensions
{
    public static StackRental<T> Rent<T>(this IStackPool<T> pool, out Stack<T> stack)
    {
        stack = pool.Rent();
        return new StackRental<T>(pool, stack);
    }
}
