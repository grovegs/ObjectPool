namespace GroveGames.ObjectPool;

public sealed class MultiTypeObjectPool<TBase> : IMultiTypeObjectPool<TBase> where TBase : class
{
    private readonly IReadOnlyDictionary<Type, IObjectPool<TBase>> _poolsByType;
    private bool _disposed;

    public MultiTypeObjectPool(IReadOnlyDictionary<Type, IObjectPool<TBase>> poolsByType)
    {
        ArgumentNullException.ThrowIfNull(poolsByType);
        ArgumentOutOfRangeException.ThrowIfZero(poolsByType.Count, nameof(poolsByType));

        _poolsByType = poolsByType;
    }

    public TBase Rent<TDerived>() where TDerived : class, TBase
    {
        var type = typeof(TDerived);
        return _poolsByType.TryGetValue(type, out var pool) ? pool.Rent() : throw new InvalidOperationException($"Type {typeof(TDerived).Name} is not registered.");
    }

    public void Return<TDerived>(TDerived item) where TDerived : class, TBase
    {
        if (_disposed)
        {
            return;
        }

        var type = typeof(TDerived);

        if (_poolsByType.TryGetValue(type, out var pool))
        {
            pool.Return(item);
        }
    }

    public int Count<TDerived>() where TDerived : class, TBase
    {
        var type = typeof(TDerived);
        return _poolsByType.TryGetValue(type, out var pool) ? pool.Count : 0;
    }

    public int MaxSize<TDerived>() where TDerived : class, TBase
    {
        var type = typeof(TDerived);
        return _poolsByType.TryGetValue(type, out var pool) ? pool.MaxSize : 0;
    }

    public void Clear()
    {
        foreach (var pool in _poolsByType.Values)
        {
            pool.Clear();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        foreach (var pool in _poolsByType.Values)
        {
            pool.Dispose();
        }
    }
}