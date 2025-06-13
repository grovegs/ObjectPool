using System.Collections.Concurrent;

namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentMultiTypeObjectPool<TBase> : IMultiTypeObjectPool<TBase> where TBase : class
{
    private readonly ConcurrentDictionary<Type, IObjectPool<TBase>> _poolsByType;
    private volatile bool _disposed;

    public ConcurrentMultiTypeObjectPool(IReadOnlyDictionary<Type, IObjectPool<TBase>> poolsByType)
    {
        ArgumentNullException.ThrowIfNull(poolsByType);
        ArgumentOutOfRangeException.ThrowIfZero(poolsByType.Count, nameof(poolsByType));

        _poolsByType = new ConcurrentDictionary<Type, IObjectPool<TBase>>(poolsByType);
        _disposed = false;
    }

    public ConcurrentMultiTypeObjectPool(ConcurrentDictionary<Type, IObjectPool<TBase>> poolsByType)
    {
        ArgumentNullException.ThrowIfNull(poolsByType);
        ArgumentOutOfRangeException.ThrowIfZero(poolsByType.Count, nameof(poolsByType));

        _poolsByType = poolsByType;
        _disposed = false;
    }

    public int Count<TDerived>() where TDerived : class, TBase
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var type = typeof(TDerived);
        return _poolsByType.TryGetValue(type, out var pool) ? pool.Count : 0;
    }

    public int MaxSize<TDerived>() where TDerived : class, TBase
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var type = typeof(TDerived);
        return _poolsByType.TryGetValue(type, out var pool) ? pool.MaxSize : 0;
    }

    public TBase Rent<TDerived>() where TDerived : class, TBase
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var type = typeof(TDerived);
        return _poolsByType.TryGetValue(type, out var pool) ? pool.Rent() : throw new InvalidOperationException($"Type {typeof(TDerived).Name} is not registered.");
    }

    public void Return<TDerived>(TDerived item) where TDerived : class, TBase
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var type = typeof(TDerived);

        if (_poolsByType.TryGetValue(type, out var pool))
        {
            pool.Return(item);
        }
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

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

        _poolsByType.Clear();
    }
}