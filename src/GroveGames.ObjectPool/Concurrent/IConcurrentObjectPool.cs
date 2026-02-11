using System;

namespace GroveGames.ObjectPool.Concurrent;

public interface IConcurrentObjectPool<TValue> : IObjectPool<TValue> where TValue : class
{
}
