using System.Collections.Generic;

namespace GroveGames.ObjectPool;

public interface IListPool<T> : IObjectPool<List<T>> where T : notnull
{
}
