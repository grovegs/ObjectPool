namespace GroveGames.ObjectPool;

public readonly struct DisposablePooledObject<T> : IDisposable
{
    private readonly Action<T> _returnAction;
    private readonly T _pooledObject;

    public DisposablePooledObject(Action<T> returnAction, T pooledObject)
    {
        _returnAction = returnAction;
        _pooledObject = pooledObject;
    }

    public void Dispose()
    {
        _returnAction.Invoke(_pooledObject);
    }
}