namespace GroveGames.ObjectPool;

public readonly ref struct StackRental<T>(IStackPool<T> pool, Stack<T> stack)
{
    private readonly IStackPool<T> _pool = pool;
    private readonly Stack<T> _stack = stack;

    public Stack<T> Value => _stack;

    public void Dispose()
    {
        _pool.Return(_stack);
    }
}
