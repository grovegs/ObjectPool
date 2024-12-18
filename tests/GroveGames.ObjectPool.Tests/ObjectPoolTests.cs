namespace GroveGames.ObjectPool.Tests;

public class ObjectPoolTests
{
    public class TestObject { }

    public class DisposableTestObject : IDisposable
    {
        private readonly IDisposable _disposable;

        public DisposableTestObject(IDisposable disposable)
        {
            _disposable = disposable;
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }

    [Fact]
    public void Get_ShouldCreateNewObject_WhenPoolIsEmpty()
    {
        // Arrange
        var strategyMock = new Mock<IPooledObjectStrategy<TestObject>>();
        var testObject = new TestObject();
        strategyMock.Setup(s => s.Create()).Returns(testObject);
        var pool = new ObjectPool<TestObject>(size: 2, strategyMock.Object);

        // Act
        var result = pool.Get();

        // Assert
        Assert.Same(testObject, result);
        strategyMock.Verify(s => s.Create(), Times.Once);
        strategyMock.Verify(s => s.Get(testObject), Times.Once);
    }

    [Fact]
    public void Return_ShouldPushObjectBackToPool()
    {
        // Arrange
        var strategyMock = new Mock<IPooledObjectStrategy<TestObject>>();
        var testObject = new TestObject();

        var pool = new ObjectPool<TestObject>(size: 2, strategyMock.Object);

        // Act
        pool.Return(testObject);
        var result = pool.Get();

        // Assert
        Assert.Same(testObject, result);
        strategyMock.Verify(s => s.Return(testObject), Times.Once);
        strategyMock.Verify(s => s.Get(testObject), Times.Once);
    }

    [Fact]
    public void Dispose_ShouldCallDisposeOnIDisposableObjects()
    {
        // Arrange
        var disposableMock = new Mock<IDisposable>();
        var disposableObject = new DisposableTestObject(disposableMock.Object);
        var strategyMock = new Mock<IPooledObjectStrategy<DisposableTestObject>>();
        strategyMock.Setup(s => s.Create()).Returns(disposableObject);
        var pool = new ObjectPool<DisposableTestObject>(size: 2, strategyMock.Object);
        pool.Return(disposableObject);

        // Act
        pool.Dispose();

        // Assert
        disposableMock.Verify(d => d.Dispose(), Times.Once);
    }

    [Fact]
    public void Dispose_ShouldClearPool()
    {
        // Arrange
        var strategyMock = new Mock<IPooledObjectStrategy<TestObject>>();
        var testObject = new TestObject();

        var pool = new ObjectPool<TestObject>(size: 2, strategyMock.Object);
        pool.Return(testObject);

        // Act
        pool.Dispose();

        // Assert
        var result = pool.Get();
        Assert.Null(result);
        strategyMock.Verify(s => s.Create(), Times.Once);
    }

    [Fact]
    public void Dispose_ShouldBeSafe_WhenCalledMultipleTimes()
    {
        // Arrange
        var disposableMock = new Mock<IDisposable>();
        var disposableObject = new DisposableTestObject(disposableMock.Object);
        var strategyMock = new Mock<IPooledObjectStrategy<DisposableTestObject>>();
        strategyMock.Setup(s => s.Create()).Returns(disposableObject);
        var pool = new ObjectPool<DisposableTestObject>(size: 2, strategyMock.Object);
        pool.Return(disposableObject);

        // Act
        pool.Dispose();
        pool.Dispose();

        // Assert
        disposableMock.Verify(d => d.Dispose(), Times.Once);
    }

    [Fact]
    public void Dispose_ShouldHandleNonIDisposableObjectsGracefully()
    {
        // Arrange
        var strategyMock = new Mock<IPooledObjectStrategy<TestObject>>();
        var testObject = new TestObject();
        strategyMock.Setup(s => s.Create()).Returns(testObject);
        var pool = new ObjectPool<TestObject>(size: 2, strategyMock.Object);
        pool.Return(testObject);

        // Act
        pool.Dispose();

        // Assert
        Assert.True(true);
    }
}