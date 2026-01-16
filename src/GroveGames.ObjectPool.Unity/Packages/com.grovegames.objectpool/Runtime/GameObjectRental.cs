using UnityEngine;

namespace GroveGames.ObjectPool.Unity
{
    public readonly ref struct GameObjectRental
    {
        private readonly IGameObjectPool _pool;
        private readonly GameObject _item;

        public GameObject Value => _item;

        public GameObjectRental(IGameObjectPool pool, GameObject item)
        {
            _pool = pool;
            _item = item;
        }

        public void Dispose()
        {
            _pool.Return(_item);
        }
    }
}
