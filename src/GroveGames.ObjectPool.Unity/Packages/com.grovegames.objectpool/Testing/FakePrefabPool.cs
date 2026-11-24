using System.Collections.Generic;
using GroveGames.ObjectPool;
using UnityEngine;

namespace GroveGames.ObjectPool.Unity.Testing
{
    public sealed class FakePrefabPool<T> : IObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly List<T> _instances = new();
        private int _returnCount;

        public int Count => 0;
        public int MaxSize => 0;
        public int ReturnCount => _returnCount;
        public IReadOnlyList<T> Rented => _instances;

        public FakePrefabPool(T prefab)
        {
            _prefab = prefab;
        }

        public T Rent()
        {
            var instance = Object.Instantiate(_prefab);
            _instances.Add(instance);
            return instance;
        }

        public void Return(T item)
        {
            _returnCount++;
        }

        public void Clear()
        {
        }

        public void Warm()
        {
        }

        public void Dispose()
        {
            for (var i = 0; i < _instances.Count; i++)
            {
                Object.DestroyImmediate(_instances[i].gameObject);
            }

            _instances.Clear();
        }
    }
}
