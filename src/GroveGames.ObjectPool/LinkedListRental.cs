namespace GroveGames.ObjectPool;

public readonly ref struct LinkedListRental<T>(ILinkedListPool<T> pool, LinkedList<T> linkedList)
{
    private readonly ILinkedListPool<T> _pool = pool;
    private readonly LinkedList<T> _linkedList = linkedList;

    public LinkedList<T> Value => _linkedList;

    public void Dispose()
    {
        _pool.Return(_linkedList);
    }
}
