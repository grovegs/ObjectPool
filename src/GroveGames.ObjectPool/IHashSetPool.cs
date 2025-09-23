using System.Collections.Generic;

namespace GroveGames.ObjectPool;

public interface IHashSetPool<T> : IObjectPool<HashSet<T>> where T : notnull
{
}