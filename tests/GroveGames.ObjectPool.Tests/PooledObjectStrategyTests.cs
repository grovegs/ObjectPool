namespace GroveGames.ObjectPool.Tests;

public class PooledObjectStrategyTests
{
    [Fact]
    public void Create_ShouldInvokeCreateDelegate_AndReturnObject()
    {
        // Arrange
        var mockObject = new object();
        var createMock = new Mock<Func<object>>();
        createMock.Setup(c => c()).Returns(mockObject);

        var strategy = new PooledObjectStrategy<object>(createMock.Object, _ => { }, _ => { });

        // Act
        var result = strategy.Create();

        // Assert
        createMock.Verify(c => c(), Times.Once);
        Assert.Same(mockObject, result);
    }

    [Fact]
    public void Get_ShouldInvokeOnGetDelegate()
    {
        // Arrange
        var pooledObject = new object();
        var onGetMock = new Mock<Action<object>>();

        var strategy = new PooledObjectStrategy<object>(() => pooledObject, onGetMock.Object, _ => { });

        // Act
        strategy.Get(pooledObject);

        // Assert
        onGetMock.Verify(a => a(pooledObject), Times.Once);
    }

    [Fact]
    public void Return_ShouldInvokeOnReturnDelegate()
    {
        // Arrange
        var pooledObject = new object();
        var onReturnMock = new Mock<Action<object>>();

        var strategy = new PooledObjectStrategy<object>(() => pooledObject, _ => { }, onReturnMock.Object);

        // Act
        strategy.Return(pooledObject);

        // Assert
        onReturnMock.Verify(a => a(pooledObject), Times.Once);
    }
}