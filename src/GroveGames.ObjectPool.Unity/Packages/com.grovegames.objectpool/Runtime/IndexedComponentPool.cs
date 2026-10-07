using System;
using UnityEngine;

namespace GroveGames.ObjectPool.Unity
{
    public sealed class IndexedComponentPool<T> : IKeyedObjectPool<int, T>
        where T : Component
    {
        private readonly IndexedObjectPool<T> _pool;
        private bool _disposed;

        public IndexedComponentPool(Func<int, T> prefabProvider, Transform parent, int initialSize, int maxSize)
        {
            if (prefabProvider == null)
            {
                Debug.LogError("Prefab provider cannot be null");
                prefabProvider = static _ => null!;
            }

            _pool = new IndexedObjectPool<T>(index => new ComponentPool<T>(prefabProvider(index), parent, initialSize, maxSize));
            _disposed = false;
        }

        public int Count(int index)
        {
            if (!IsUsable(index))
            {
                return 0;
            }

            return _pool.Count(index);
        }

        public int MaxSize(int index)
        {
            if (!IsUsable(index))
            {
                return 0;
            }

            return _pool.MaxSize(index);
        }

        public T Rent(int index)
        {
            if (!IsUsable(index))
            {
                return null!;
            }

            return _pool.Rent(index);
        }

        public void Return(int index, T item)
        {
            if (!IsUsable(index))
            {
                return;
            }

            if (item == null)
            {
                Debug.LogError("Returned item cannot be null");
                return;
            }

            _pool.Return(index, item);
        }

        public void Warm(int index)
        {
            if (!IsUsable(index))
            {
                return;
            }

            _pool.Warm(index);
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

        public void Clear(int index)
        {
            if (!IsUsable(index))
            {
                return;
            }

            _pool.Clear(index);
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

        private bool IsUsable(int index)
        {
            if (_disposed)
            {
                Debug.LogError($"{GetType().FullName} is disposed");
                return false;
            }

            if (index < 0)
            {
                Debug.LogError($"Index cannot be negative: {index}");
                return false;
            }

            return true;
        }
    }
}
