using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GroveGames.ObjectPool.Unity
{
    public sealed class GameObjectPool : IGameObjectPool
    {
        private readonly ObjectPool<GameObject> _pool;
        private bool _disposed;

        public int Count
        {
            get
            {
                ThrowIfDisposed();
                return _pool.Count;
            }
        }

        public int MaxSize
        {
            get
            {
                ThrowIfDisposed();
                return _pool.MaxSize;
            }
        }

        public GameObjectPool(GameObject prefab, Transform parent, int initialSize, int maxSize)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            if (initialSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialSize));
            }

            if (initialSize > maxSize)
            {
                throw new ArgumentOutOfRangeException(nameof(initialSize));
            }

            if (maxSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxSize));
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
            ThrowIfDisposed();
            return _pool.Rent();
        }

        public void Return(GameObject gameObject)
        {
            ThrowIfDisposed();
            _pool.Return(gameObject);
        }

        public void Clear()
        {
            ThrowIfDisposed();
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

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
