namespace GroveGames.ObjectPool;

public interface IQueuePool<T> : IObjectPool<Queue<T>> where T : notnull
{
}
