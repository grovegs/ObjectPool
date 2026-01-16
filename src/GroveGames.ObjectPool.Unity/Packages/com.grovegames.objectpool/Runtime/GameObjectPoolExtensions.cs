using UnityEngine;

namespace GroveGames.ObjectPool.Unity
{
    public static class GameObjectPoolExtensions
    {
        public static GameObjectRental Rent(this IGameObjectPool pool, out GameObject item)
        {
            item = pool.Rent();
            return new GameObjectRental(pool, item);
        }
    }
}
