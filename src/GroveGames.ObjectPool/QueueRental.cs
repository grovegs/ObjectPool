namespace GroveGames.ObjectPool;

public readonly ref struct QueueRental<T>(IQueuePool<T> pool, Queue<T> queue)
{
    private readonly IQueuePool<T> _pool = pool;
    private readonly Queue<T> _queue = queue;

    public Queue<T> Value => _queue;

    public void Dispose()
    {
        _pool.Return(_queue);
    }
}
