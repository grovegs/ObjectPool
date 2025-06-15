namespace GroveGames.ObjectPool;

public interface IStackPool<T> : IObjectPool<Stack<T>> where T : notnull
{
}
