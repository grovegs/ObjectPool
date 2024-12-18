using GroveGames.ObjectPool.Pools;

namespace GroveGames.ObjectPool.Tests.Pools;

public class ListPooledObjectStrategyTests
{
    [Fact]
    public void Create_ShouldReturnList_WithSpecifiedCapacity()
    {
        // Arrange
        int size = 10;
        var strategy = new ListPooledObjectStrategy<int>(size);

        // Act
        var list = strategy.Create();

        // Assert
        Assert.NotNull(list);
        Assert.Empty(list);
        Assert.Equal(size, list.Capacity);
    }

    [Fact]
    public void Get_ShouldNotModifyList()
    {
        // Arrange
        var strategy = new ListPooledObjectStrategy<int>(10);
        var list = new List<int> { 1, 2, 3 };

        // Act
        strategy.Get(list);

        // Assert
        Assert.Equal(3, list.Count);
        int[] expected = [1, 2, 3];
        Assert.Equal(expected, list);
    }

    [Fact]
    public void Return_ShouldClearList()
    {
        // Arrange
        var strategy = new ListPooledObjectStrategy<int>(10);
        var list = new List<int> { 1, 2, 3 };

        // Act
        strategy.Return(list);

        // Assert
        Assert.Empty(list);
    }
}
