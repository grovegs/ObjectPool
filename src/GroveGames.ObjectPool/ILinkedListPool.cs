using System.Collections.Generic;

namespace GroveGames.ObjectPool;

public interface ILinkedListPool<T> : IObjectPool<LinkedList<T>> where T : notnull
{
}
