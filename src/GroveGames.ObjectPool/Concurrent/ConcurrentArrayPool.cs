using System.Collections.Concurrent;
using System.Threading;

namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentArrayPool<T> : IArrayPool<T> where T : notnull
{
    private readonly ConcurrentDictionary<int, ConcurrentObjectPool<T[]>> _poolsBySize;
    private readonly int _initialSize;
    private readonly int _maxSize;
    private volatile int _disposed;

    public ConcurrentArrayPool(int initialSize, int maxSize)
    {
        ThrowHelper.ThrowIfNegative(initialSize);
        ThrowHelper.ThrowIfGreaterThan(initialSize, maxSize);
        ThrowHelper.ThrowIfNegativeOrZero(maxSize);

        _poolsBySize = new ConcurrentDictionary<int, ConcurrentObjectPool<T[]>>();
        _initialSize = initialSize;
        _maxSize = maxSize;
        _disposed = 0;
    }

    public int Count(int size)
    {
        ThrowHelper.ThrowIfDisposed(_disposed == 1, this);

        return _poolsBySize.TryGetValue(size, out var pool) ? pool.Count : 0;
    }

    public int MaxSize(int size)
    {
        ThrowHelper.ThrowIfDisposed(_disposed == 1, this);

        return _poolsBySize.ContainsKey(size) || size > 0 ? _maxSize : 0;
    }

    public T[] Rent(int size)
    {
        ThrowHelper.ThrowIfDisposed(_disposed == 1, this);

        if (size == 0)
        {
            return [];
        }

        var pool = _poolsBySize.GetOrAdd(size, s => new ConcurrentObjectPool<T[]>(() => new T[s], null, null, _initialSize, _maxSize));
        return pool.Rent();
    }

    public void Return(T[] array)
    {
        ThrowHelper.ThrowIfDisposed(_disposed == 1, this);

        var size = array.Length;

        if (_poolsBySize.TryGetValue(size, out var pool))
        {
            pool.Return(array);
        }
    }

    public void Clear()
    {
        ThrowHelper.ThrowIfDisposed(_disposed == 1, this);

        foreach (var kvp in _poolsBySize)
        {
            kvp.Value.Clear();
        }

        _poolsBySize.Clear();
    }

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
        {
            return;
        }

        foreach (var kvp in _poolsBySize)
        {
            kvp.Value.Dispose();
        }

        _poolsBySize.Clear();
    }
}