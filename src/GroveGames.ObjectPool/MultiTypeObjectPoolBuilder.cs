using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace GroveGames.ObjectPool;

public sealed class MultiTypeObjectPoolBuilder<TBase> where TBase : class
{
    private readonly Dictionary<Type, IObjectPool<TBase>> _poolsByType;

    public MultiTypeObjectPoolBuilder()
    {
        _poolsByType = new Dictionary<Type, IObjectPool<TBase>>();
    }

    public MultiTypeObjectPoolBuilder<TBase> AddPool<TDerived>(Func<TDerived> factory, Action<TDerived>? onRent, Action<TDerived>? onReturn, int initialSize, int maxSize) where TDerived : class, TBase
    {
        var pool = new TypedObjectPool<TBase, TDerived>(factory, onRent, onReturn, initialSize, maxSize);
        var type = typeof(TDerived);

        if (!_poolsByType.TryAdd(type, pool))
        {
            throw new ArgumentException($"Pool for type {type.Name} has already been added.");
        }

        return this;
    }

    public IReadOnlyDictionary<Type, IObjectPool<TBase>> Build()
    {
        return _poolsByType.ToFrozenDictionary();
    }
}