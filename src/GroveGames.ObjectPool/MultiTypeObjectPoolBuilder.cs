using System.Collections.Frozen;
using System.Runtime.InteropServices;

namespace GroveGames.ObjectPool;

public sealed class MultiTypeObjectPoolBuilder<TBase> where TBase : class
{
    private readonly Dictionary<Type, IObjectPool<TBase>> _poolsByType;

    public MultiTypeObjectPoolBuilder()
    {
        _poolsByType = [];
    }

    public MultiTypeObjectPoolBuilder<TBase> AddPool<TDerived>(Func<TDerived> factory, Action<TDerived>? onRent, Action<TDerived>? onReturn, int initialSize, int maxSize) where TDerived : class, TBase
    {
        var pool = new TypedObjectPool<TBase, TDerived>(factory, onRent, onReturn, initialSize, maxSize);
        var type = typeof(TDerived);
        ref var location = ref CollectionsMarshal.GetValueRefOrAddDefault(_poolsByType, type, out bool exists);

        if (exists)
        {
            throw new ArgumentException($"Pool for type {type.Name} has already been added.");
        }

        location = pool;
        return this;
    }

    public FrozenDictionary<Type, IObjectPool<TBase>> Build()
    {
        return _poolsByType.ToFrozenDictionary();
    }
}
