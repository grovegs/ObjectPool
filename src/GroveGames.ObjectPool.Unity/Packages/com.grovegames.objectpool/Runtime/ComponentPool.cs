using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GroveGames.ObjectPool.Unity
{
    public sealed class ComponentPool<T> : IObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly int _initialSize;
        private readonly int _maxSize;
        private readonly Stack<T> _items;
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

        public ComponentPool(T prefab, Transform parent, int initialSize, int maxSize)
        {
            if (prefab == null)
            {
                Debug.LogError("Prefab cannot be null");
                prefab = new GameObject("NullPrefab").AddComponent<T>();
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
            _items = new Stack<T>(initialSize);
            _disposed = false;
        }

        public T Rent()
        {
            if (_disposed)
            {
                Debug.LogError($"{GetType().FullName} is disposed");
                return null!;
            }

            while (_items.Count > 0)
            {
                var pooled = _items.Pop();

                if (pooled != null)
                {
                    pooled.gameObject.SetActive(true);
                    return pooled;
                }
            }

            var created = Create();
            created.gameObject.SetActive(true);
            return created;
        }

        public void Return(T item)
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

            var gameObject = item.gameObject;
            gameObject.SetActive(false);

            if (_items.Count >= _maxSize)
            {
                DestroyObject(gameObject);
                return;
            }

            var transform = item.transform;

            if (transform.parent != _parent)
            {
                transform.SetParent(_parent, false);
            }

            _items.Push(item);
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

            for (var i = 0; i < _initialSize && _items.Count < _maxSize; i++)
            {
                var created = Create();
                created.gameObject.SetActive(false);
                _items.Push(created);
            }
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

        private T Create()
        {
            return _parent != null ? Object.Instantiate(_prefab, _parent) : Object.Instantiate(_prefab);
        }

        private void DestroyItems()
        {
            while (_items.Count > 0)
            {
                var item = _items.Pop();

                if (item != null)
                {
                    DestroyObject(item.gameObject);
                }
            }
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
