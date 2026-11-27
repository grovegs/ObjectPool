using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GroveGames.ObjectPool.Unity
{
    public sealed class GameObjectPool : IObjectPool<GameObject>
    {
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly int _initialSize;
        private readonly int _maxSize;
        private readonly List<GameObject> _items;
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

                RemoveDestroyed();
                return _items.Count;
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

                return _maxSize;
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

            _prefab = prefab;
            _parent = parent;
            _initialSize = initialSize;
            _maxSize = maxSize;
            _items = new List<GameObject>(initialSize);
            _disposed = false;
        }

        public GameObject Rent()
        {
            if (_disposed)
            {
                Debug.LogError($"{GetType().FullName} is disposed");
                return null!;
            }

            while (_items.Count > 0)
            {
                var last = _items.Count - 1;
                var pooled = _items[last];
                _items.RemoveAt(last);

                if (pooled != null)
                {
                    pooled.SetActive(true);
                    return pooled;
                }
            }

            var created = Create();
            created.SetActive(true);
            return created;
        }

        public void Return(GameObject item)
        {
            if (_disposed)
            {
                Debug.LogError($"{GetType().FullName} is disposed");
                return;
            }

            if (item == null)
            {
                Debug.LogError("Returned item cannot be null");
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_items.Contains(item))
            {
                Debug.LogError($"{item.name} was returned to the pool twice");
                return;
            }
#endif

            item.SetActive(false);

            RemoveDestroyed();

            if (_items.Count >= _maxSize)
            {
                DestroyObject(item);
                return;
            }

            var transform = item.transform;

            if (transform.parent != _parent)
            {
                transform.SetParent(_parent, false);
            }

            _items.Add(item);
        }

        public void Clear()
        {
            if (_disposed)
            {
                Debug.LogError($"{GetType().FullName} is disposed");
                return;
            }

            DestroyItems();
        }

        public void Warm()
        {
            if (_disposed)
            {
                Debug.LogError($"{GetType().FullName} is disposed");
                return;
            }

            RemoveDestroyed();

            if (_initialSize == 0 || _items.Count >= _maxSize)
            {
                return;
            }

            var staging = new GameObject("Staging");
            staging.SetActive(false);

            for (var i = 0; i < _initialSize && _items.Count < _maxSize; i++)
            {
                var created = Object.Instantiate(_prefab, staging.transform);
                created.SetActive(false);
                created.transform.SetParent(_parent, false);
                _items.Add(created);
            }

            DestroyObject(staging);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            DestroyItems();
        }

        private GameObject Create()
        {
            return _parent != null ? Object.Instantiate(_prefab, _parent) : Object.Instantiate(_prefab);
        }

        private void DestroyItems()
        {
            while (_items.Count > 0)
            {
                var last = _items.Count - 1;
                var item = _items[last];
                _items.RemoveAt(last);

                if (item != null)
                {
                    DestroyObject(item);
                }
            }
        }

        private void RemoveDestroyed()
        {
            _items.RemoveAll(static item => item == null);
        }

        private static void DestroyObject(GameObject gameObject)
        {
            if (Application.isPlaying)
            {
                Object.Destroy(gameObject);
            }
            else
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
