using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;

namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentMultiTypeObjectPoolBuilder<TBase> where TBase : class
{
    private readonly ConcurrentDictionary<Type, IObjectPool<TBase>> _poolsByType;

    public ConcurrentMultiTypeObjectPoolBuilder()
    {
        _poolsByType = new ConcurrentDictionary<Type, IObjectPool<TBase>>();
    }

    public ConcurrentMultiTypeObjectPoolBuilder<TBase> AddPool<TDerived>(Func<TDerived> factory, Action<TDerived>? onRent, Action<TDerived>? onReturn, int initialSize, int maxSize) where TDerived : class, TBase
    {
        var pool = new ConcurrentTypedObjectPool<TBase, TDerived>(factory, onRent, onReturn, initialSize, maxSize);
        var type = typeof(TDerived);

        if (!_poolsByType.TryAdd(type, pool))
        {
            throw new ArgumentException($"Pool for type {type.Name} has already been added.");
        }

        return this;
    }

    public FrozenDictionary<Type, IObjectPool<TBase>> Build()
    {
        return _poolsByType.ToFrozenDictionary();
    }
}
