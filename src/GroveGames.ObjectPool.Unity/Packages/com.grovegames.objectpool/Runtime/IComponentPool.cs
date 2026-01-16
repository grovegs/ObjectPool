using UnityEngine;

namespace GroveGames.ObjectPool.Unity
{
    public interface IComponentPool<T> : IObjectPool<T> where T : Component
    {
    }
}
