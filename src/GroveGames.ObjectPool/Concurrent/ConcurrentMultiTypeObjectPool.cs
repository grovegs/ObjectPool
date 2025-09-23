using System;
using System.Collections.Generic;
using System.Threading;

namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentMultiTypeObjectPool<TBase> : IMultiTypeObjectPool<TBase> where TBase : class
{
    private readonly IReadOnlyDictionary<Type, IObjectPool<TBase>> _poolsByType;
    private volatile int _disposed;

    public ConcurrentMultiTypeObjectPool(Action<ConcurrentMultiTypeObjectPoolBuilder<TBase>> configure)
    {
        ThrowHelper.ThrowIfNull(configure);

        var builder = new ConcurrentMultiTypeObjectPoolBuilder<TBase>();
        configure(builder);
        _poolsByType = builder.Build();
        _disposed = 0;
    }

    public int Count<TDerived>() where TDerived : class, TBase
    {
        ThrowHelper.ThrowIfDisposed(_disposed == 1, this);

        var type = typeof(TDerived);
        return _poolsByType.TryGetValue(type, out var pool) ? pool.Count : 0;
    }

    public int MaxSize<TDerived>() where TDerived : class, TBase
    {
        ThrowHelper.ThrowIfDisposed(_disposed == 1, this);

        var type = typeof(TDerived);
        return _poolsByType.TryGetValue(type, out var pool) ? pool.MaxSize : 0;
    }

    public TBase Rent<TDerived>() where TDerived : class, TBase
    {
        ThrowHelper.ThrowIfDisposed(_disposed == 1, this);

        var type = typeof(TDerived);
        return _poolsByType.TryGetValue(type, out var pool) ? pool.Rent() : throw new InvalidOperationException($"Type {typeof(TDerived).Name} is not registered.");
    }

    public void Return<TDerived>(TDerived item) where TDerived : class, TBase
    {
        ThrowHelper.ThrowIfDisposed(_disposed == 1, this);

        var type = typeof(TDerived);

        if (_poolsByType.TryGetValue(type, out var pool))
        {
            pool.Return(item);
        }
    }

    public void Clear()
    {
        ThrowHelper.ThrowIfDisposed(_disposed == 1, this);

        foreach (var pool in _poolsByType.Values)
        {
            pool.Clear();
        }
    }

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
        {
            return;
        }

        foreach (var pool in _poolsByType.Values)
        {
            pool.Dispose();
        }
    }
}