namespace GroveGames.ObjectPool.Pools;

public sealed class ListPooledObjectStrategy<T> : IPooledObjectStrategy<List<T>>
{
    private readonly int _size;

    public ListPooledObjectStrategy(int size)
    {
        _size = size;
    }

    public List<T> Create()
    {
        return new List<T>(_size);
    }

    public void Get(List<T> list)
    {
    }

    public void Return(List<T> list)
    {
        list.Clear();
    }
}
