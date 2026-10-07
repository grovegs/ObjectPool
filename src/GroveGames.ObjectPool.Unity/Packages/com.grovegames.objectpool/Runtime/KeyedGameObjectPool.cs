using System;
using UnityEngine;

namespace GroveGames.ObjectPool.Unity
{
    public sealed class KeyedGameObjectPool<TKey> : IKeyedObjectPool<TKey, GameObject>
        where TKey : notnull
    {
        private readonly KeyedObjectPool<TKey, GameObject> _pool;
        private bool _disposed;

        public KeyedGameObjectPool(Func<TKey, GameObject> prefabProvider, Transform parent, int initialSize, int maxSize)
        {
            if (prefabProvider == null)
            {
                Debug.LogError("Prefab provider cannot be null");
                prefabProvider = static _ => null!;
            }

            _pool = new KeyedObjectPool<TKey, GameObject>(key => new GameObjectPool(prefabProvider(key), parent, initialSize, maxSize));
            _disposed = false;
        }

        public int Count(TKey key)
        {
            if (!IsUsable(key))
            {
                return 0;
            }

            return _pool.Count(key);
        }

        public int MaxSize(TKey key)
        {
            if (!IsUsable(key))
            {
                return 0;
            }

            return _pool.MaxSize(key);
        }

        public GameObject Rent(TKey key)
        {
            if (!IsUsable(key))
            {
                return null!;
            }

            return _pool.Rent(key);
        }

        public void Return(TKey key, GameObject item)
        {
            if (!IsUsable(key))
            {
                return;
            }

            if (item == null)
            {
                Debug.LogError("Returned item cannot be null");
                return;
            }

            _pool.Return(key, item);
        }

        public void Warm(TKey key)
        {
            if (!IsUsable(key))
            {
                return;
            }

            _pool.Warm(key);
        }

        public void Warm()
        {
            if (_disposed)
            {
                Debug.LogError($"{GetType().FullName} is disposed");
                return;
            }

            _pool.Warm();
        }

        public void Clear(TKey key)
        {
            if (!IsUsable(key))
            {
                return;
            }

            _pool.Clear(key);
        }

        public void Clear()
        {
            if (_disposed)
            {
                Debug.LogError($"{GetType().FullName} is disposed");
                return;
            }

            _pool.Clear();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _pool.Dispose();
        }

        private bool IsUsable(TKey key)
        {
            if (_disposed)
            {
                Debug.LogError($"{GetType().FullName} is disposed");
                return false;
            }

            if (key == null)
            {
                Debug.LogError("Key cannot be null");
                return false;
            }

            return true;
        }
    }
}
