namespace GroveGames.ObjectPool;

public readonly struct DisposablePooledObject<T> : IDisposable where T : class
{
    private readonly Action<T> _onReturn;
    private readonly T _pooledObject;

    public DisposablePooledObject(Action<T> onReturn, T pooledObject)
    {
        _onReturn = onReturn;
        _pooledObject = pooledObject;
    }

    public void Dispose()
    {
        _onReturn.Invoke(_pooledObject);
    }
}