using UnityEngine;

namespace GroveGames.ObjectPool.Unity
{
    public readonly ref struct ComponentRental<T> where T : Component
    {
        private readonly IComponentPool<T> _pool;
        private readonly T _item;

        public T Value => _item;

        public ComponentRental(IComponentPool<T> pool, T item)
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
