using GroveGames.ObjectPool.Pools;

namespace GroveGames.ObjectPool.Tests.Pools;

public interface IPooledItem { }
public class ConcretePooledItemA : IPooledItem { }
public class ConcretePooledItemB : IPooledItem { }

public class MultiTypeObjectPoolTests
{
    [Fact]
    public void AddPooledObjectStrategy_ShouldRegisterStrategyForInterfaceBase()
    {
        // Arrange
        var mockStrategy = new Mock<IPooledObjectStrategy<IPooledItem>>();
        var mockObject = new ConcretePooledItemA();
        mockStrategy.Setup(s => s.Create()).Returns(mockObject);

        var pool = new MultiTypeObjectPool<IPooledItem>();

        // Act
        pool.AddPooledObjectStrategy<ConcretePooledItemA>(mockStrategy.Object);

        // Assert
        var exception = Record.Exception(() => pool.Get<ConcretePooledItemA>());
        Assert.Null(exception); // No exception means the strategy was successfully registered
    }

    [Fact]
    public void RemovePooledObjectStrategy_ShouldUnregisterStrategyForInterfaceBase()
    {
        // Arrange
        var mockStrategy = new Mock<IPooledObjectStrategy<IPooledItem>>();
        var pool = new MultiTypeObjectPool<IPooledItem>();
        pool.AddPooledObjectStrategy<ConcretePooledItemA>(mockStrategy.Object);

        // Act
        pool.RemovePooledObjectStrategy<ConcretePooledItemA>();

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(() => pool.Get<ConcretePooledItemA>());
        Assert.Contains("No pooling strategy registered", exception.Message);
    }

    [Fact]
    public void Get_ShouldRetrieveObjectFromPoolWhenBaseIsInterface()
    {
        // Arrange
        var mockStrategy = new Mock<IPooledObjectStrategy<IPooledItem>>();
        var mockObject = new ConcretePooledItemA();
        mockStrategy.Setup(s => s.Create()).Returns(mockObject);

        var pool = new MultiTypeObjectPool<IPooledItem>();
        pool.AddPooledObjectStrategy<ConcretePooledItemA>(mockStrategy.Object);

        // Act
        var result = pool.Get<ConcretePooledItemA>();

        // Assert
        Assert.NotNull(result);
        Assert.Same(mockObject, result);
        mockStrategy.Verify(s => s.Create(), Times.Once);
    }

    [Fact]
    public void Return_ShouldReturnObjectToPoolWhenBaseIsInterface()
    {
        // Arrange
        var mockStrategy = new Mock<IPooledObjectStrategy<IPooledItem>>();
        var mockObject = new ConcretePooledItemA();
        mockStrategy.Setup(s => s.Create()).Returns(mockObject);
        mockStrategy.Setup(s => s.Return(It.IsAny<IPooledItem>()));

        var pool = new MultiTypeObjectPool<IPooledItem>();
        pool.AddPooledObjectStrategy<ConcretePooledItemA>(mockStrategy.Object);
        pool.Get<ConcretePooledItemA>();

        // Act
        pool.Return(mockObject);

        // Assert
        mockStrategy.Verify(s => s.Return(mockObject), Times.Once);
    }

    [Fact]
    public void Get_WithDisposable_ShouldDisposeProperlyWhenBaseIsInterface()
    {
        // Arrange
        var mockStrategy = new Mock<IPooledObjectStrategy<IPooledItem>>();
        var mockObject = new ConcretePooledItemA();
        mockStrategy.Setup(s => s.Create()).Returns(mockObject);

        var pool = new MultiTypeObjectPool<IPooledItem>();
        pool.AddPooledObjectStrategy<ConcretePooledItemA>(mockStrategy.Object);

        // Act
        using (pool.Get(out ConcretePooledItemA result))
        {
            Assert.NotNull(result);
            Assert.Same(mockObject, result);
        }

        // Assert
        mockStrategy.Verify(s => s.Return(mockObject), Times.Once);
    }

    [Fact]
    public void Dispose_ShouldClearAllPoolsAndStrategies()
    {
        // Arrange
        var mockStrategyA = new Mock<IPooledObjectStrategy<IPooledItem>>();
        var mockStrategyB = new Mock<IPooledObjectStrategy<IPooledItem>>();
        var mockObjectA = new ConcretePooledItemA();
        var mockObjectB = new ConcretePooledItemB();
        mockStrategyA.Setup(s => s.Create()).Returns(mockObjectA);
        mockStrategyB.Setup(s => s.Create()).Returns(mockObjectB);
        var pool = new MultiTypeObjectPool<IPooledItem>();
        pool.AddPooledObjectStrategy<ConcretePooledItemA>(mockStrategyA.Object);
        pool.AddPooledObjectStrategy<ConcretePooledItemB>(mockStrategyB.Object);
        var retrievedA = pool.Get<ConcretePooledItemA>();
        var retrievedB = pool.Get<ConcretePooledItemB>();

        // Act
        pool.Dispose();

        // Assert
        Assert.Throws<InvalidOperationException>(() => pool.Get<ConcretePooledItemA>());
        Assert.Throws<InvalidOperationException>(() => pool.Get<ConcretePooledItemB>());
    }
}