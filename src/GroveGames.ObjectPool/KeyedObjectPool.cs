using System;
using System.Collections.Generic;

namespace GroveGames.ObjectPool
{
    public sealed class KeyedObjectPool<TKey, TValue, TPool> : IKeyedObjectPool<TKey, TValue>
        where TPool : IObjectPool<TValue>
    {
        private readonly Dictionary<TKey, TPool> _pools;
        private readonly Func<TKey, TPool> _poolFactory;
        private bool _disposed;

        public int PoolCount => _disposed ? 0 : _pools.Count;

        public KeyedObjectPool(Func<TKey, TPool> poolFactory)
        {
            _poolFactory = poolFactory ?? throw new ArgumentNullException(nameof(poolFactory));
            _pools = new Dictionary<TKey, TPool>();
            _disposed = false;
        }

        public void Warm(TKey key)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (_pools.ContainsKey(key))
            {
                return;
            }

            var pool = _poolFactory(key);
            if (pool == null)
            {
                throw new InvalidOperationException($"Pool factory returned null for key: {key}");
            }

            _pools[key] = pool;
        }

        public TValue Rent(TKey key)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (!_pools.TryGetValue(key, out var pool))
            {
                Warm(key);
                pool = _pools[key];
            }

            return pool.Rent();
        }

        public void Return(TKey key, TValue item)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (_pools.TryGetValue(key, out var pool))
            {
                pool.Return(item);
            }
        }

        public void Clear()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }
        }

        public void ClearKey(TKey key)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (_pools.TryGetValue(key, out var pool))
            {
                pool.Clear();
            }
        }

        public bool ContainsKey(TKey key)
        {
            return !_disposed && _pools.ContainsKey(key);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            foreach (var pool in _pools.Values)
            {
                pool?.Dispose();
            }

            _pools.Clear();
        }
    }
}
