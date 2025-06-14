namespace GroveGames.ObjectPool.Concurrent;

public sealed class ConcurrentStackPool<T> : IStackPool<T> where T : notnull
{
    private readonly ConcurrentObjectPool<Stack<T>> _pool;
    private volatile bool _disposed;

    public int Count => _disposed ? throw new ObjectDisposedException(nameof(ConcurrentStackPool<T>)) : _pool.Count;
    public int MaxSize => _disposed ? throw new ObjectDisposedException(nameof(ConcurrentStackPool<T>)) : _pool.MaxSize;

    public ConcurrentStackPool(int capacity, int maxSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);

        _pool = new ConcurrentObjectPool<Stack<T>>(() => new Stack<T>(capacity), static stack => stack.Clear(), maxSize);
        _disposed = false;
    }

    public Stack<T> Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _pool.Rent();
    }

    public void Return(Stack<T> stack)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _pool.Return(stack);
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _pool.Clear();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _pool.Dispose();
    }
}
