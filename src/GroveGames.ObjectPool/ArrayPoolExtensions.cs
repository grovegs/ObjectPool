namespace GroveGames.ObjectPool;

public static class ArrayPoolExtensions
{
    public static ArrayRental<T> Rent<T>(this IArrayPool<T> pool, int size, out T[] array) where T : notnull
    {
        array = pool.Rent(size);
        return new ArrayRental<T>(pool, array);
    }
}
