using System;

using GroveGames.DependencyInjection;
using GroveGames.DependencyInjection.Unity;

using UnityEngine;

namespace GroveGames.ObjectPool.Unity
{
    public static class ObjectPoolContainerBuilderExtensions
    {
        public static IContainerBuilder AddComponentPool<T>(this IContainerBuilder builder, T prefab, int initialSize, int maxSize)
            where T : Component
        {
            return builder.AddSingleton<IObjectPool<T>>(resolver => new ComponentPool<T>(prefab, CreateRoot(resolver, typeof(T)), initialSize, maxSize));
        }

        public static IContainerBuilder AddGameObjectPool(this IContainerBuilder builder, GameObject prefab, int initialSize, int maxSize)
        {
            return builder.AddSingleton<IObjectPool<GameObject>>(resolver => new GameObjectPool(prefab, CreateRoot(resolver, typeof(GameObject)), initialSize, maxSize));
        }

        public static IContainerBuilder AddKeyedComponentPool<TKey, T>(this IContainerBuilder builder, Func<TKey, T> prefabProvider, int initialSize, int maxSize)
            where TKey : notnull
            where T : Component
        {
            return builder.AddSingleton<IKeyedObjectPool<TKey, T>>(resolver => new KeyedComponentPool<TKey, T>(prefabProvider, CreateRoot(resolver, typeof(T)), initialSize, maxSize));
        }

        public static IContainerBuilder AddKeyedGameObjectPool<TKey>(this IContainerBuilder builder, Func<TKey, GameObject> prefabProvider, int initialSize, int maxSize)
            where TKey : notnull
        {
            return builder.AddSingleton<IKeyedObjectPool<TKey, GameObject>>(resolver => new KeyedGameObjectPool<TKey>(prefabProvider, CreateRoot(resolver, typeof(GameObject)), initialSize, maxSize));
        }

        public static IContainerBuilder AddIndexedComponentPool<T>(this IContainerBuilder builder, Func<int, T> prefabProvider, int initialSize, int maxSize)
            where T : Component
        {
            return builder.AddSingleton<IKeyedObjectPool<int, T>>(resolver => new IndexedComponentPool<T>(prefabProvider, CreateRoot(resolver, typeof(T)), initialSize, maxSize));
        }

        public static IContainerBuilder AddIndexedGameObjectPool(this IContainerBuilder builder, Func<int, GameObject> prefabProvider, int initialSize, int maxSize)
        {
            return builder.AddSingleton<IKeyedObjectPool<int, GameObject>>(resolver => new IndexedGameObjectPool(prefabProvider, CreateRoot(resolver, typeof(GameObject)), initialSize, maxSize));
        }

        private static Transform CreateRoot(IObjectResolver resolver, Type type)
        {
            var root = resolver.Instantiate<PoolRoot>();
            root.name = $"{type.Name} Pool";
            return root.transform;
        }
    }
}
