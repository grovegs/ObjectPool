using GroveGames.ObjectPool;

namespace GroveGames.ObjectPool.Testing
{
    public sealed class FakeObjectPool<T> : IObjectPool<T> where T : class
    {
        public int Count => 0;
        public int MaxSize => 0;

        public T Rent()
        {
            return null!;
        }

        public void Return(T item)
        {
        }

        public void Clear()
        {
        }

        public void Warm()
        {
        }

        public void Dispose()
        {
        }
    }
}
