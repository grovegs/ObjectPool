using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GroveGames.ObjectPool.Unity
{
    public sealed class GameObjectPool : IObjectPool<GameObject>
    {
        private readonly ObjectPool<GameObject> _pool;
        private bool _disposed;

        public int Count
        {
            get
            {
                if (_disposed)
                {
                    Debug.LogError($"{GetType().FullName} is disposed");
                    return 0;
                }
                return _pool.Count;
            }
        }

        public int MaxSize
        {
            get
            {
                if (_disposed)
                {
                    Debug.LogError($"{GetType().FullName} is disposed");
                    return 0;
                }
                return _pool.MaxSize;
            }
        }

        public GameObjectPool(GameObject prefab, Transform parent, int initialSize, int maxSize)
        {
            if (prefab == null)
            {
                Debug.LogError("Prefab cannot be null");
                prefab = new GameObject("NullPrefab");
            }

            if (initialSize < 0)
            {
                Debug.LogError($"Initial size cannot be negative: {initialSize}");
                initialSize = 0;
            }

            if (maxSize <= 0)
            {
                Debug.LogError($"Max size must be positive: {maxSize}");
                maxSize = 10;
            }

            if (initialSize > maxSize)
            {
                Debug.LogError($"Initial size {initialSize} cannot exceed max size {maxSize}");
                initialSize = maxSize;
            }

            _pool = new ObjectPool<GameObject>(
                () => parent != null ? Object.Instantiate(prefab, parent) : Object.Instantiate(prefab),
                static gameObject => gameObject.SetActive(true),
                static gameObject => gameObject.SetActive(false),
                initialSize,
                maxSize);

            _disposed = false;
        }

        public GameObject Rent()
        {
            if (_disposed)
            {
                Debug.LogError($"{GetType().FullName} is disposed");
                return null;
            }
            return _pool.Rent();
        }

        public void Return(GameObject gameObject)
        {
            if (_disposed)
            {
                Debug.LogError($"{GetType().FullName} is disposed");
                return;
            }
            _pool.Return(gameObject);
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

        public void Warm()
        {
            if (_disposed)
            {
                Debug.LogError($"{GetType().FullName} is disposed");
                return;
            }
            _pool.Warm();
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
    }
}
