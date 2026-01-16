using UnityEngine;

namespace GroveGames.ObjectPool.Unity
{
    public static class ComponentPoolExtensions
    {
        public static ComponentRental<T> Rent<T>(this IComponentPool<T> pool, out T item) where T : Component
        {
            item = pool.Rent();
            return new ComponentRental<T>(pool, item);
        }
    }
}
