using GroveGames.ObjectPool.Pools;

namespace GroveGames.ObjectPool.Tests.Pools;

public class ArrayPooledObjectStrategyTests
{
    [Fact]
    public void Create_ShouldReturnArray_WithCorrectSize()
    {
        // Arrange
        int size = 128;
        var strategy = new ArrayPooledObjectStrategy<int>(size);

        // Act
        var array = strategy.Create();

        // Assert
        Assert.NotNull(array);
        Assert.Equal(size, array.Length);
        Assert.All(array, item => Assert.Equal(0, item));
    }

    [Fact]
    public void Get_ShouldNotModifyArray()
    {
        // Arrange
        int size = 128;
        var strategy = new ArrayPooledObjectStrategy<int>(size);
        var array = new int[size];
        array[0] = 42;

        // Act
        strategy.Get(array);

        // Assert
        Assert.Equal(42, array[0]);
    }

    [Fact]
    public void Return_ShouldClearArray()
    {
        // Arrange
        int size = 128;
        var strategy = new ArrayPooledObjectStrategy<int>(size);
        var array = new int[size];

        for (int i = 0; i < size; i++)
        {
            array[i] = 1;
        }

        // Act
        strategy.Return(array);

        // Assert
        Assert.All(array, item => Assert.Equal(0, item));
    }
}