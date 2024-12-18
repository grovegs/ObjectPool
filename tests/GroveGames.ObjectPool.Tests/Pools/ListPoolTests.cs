using GroveGames.ObjectPool.Pools;

namespace GroveGames.ObjectPool.Tests.Pools;

public class ListPoolTests
{
    [Fact]
    public void Get_ShouldReturnEmptyList()
    {
        // Arrange
        int poolSize = 10;
        var listPool = new ListPool<int>(poolSize);

        // Act
        var list = listPool.Get();

        // Assert
        Assert.NotNull(list);
        Assert.Empty(list);
    }

    [Fact]
    public void Return_ShouldClearList_WhenReturnedToPool()
    {
        // Arrange
        int poolSize = 10;
        var listPool = new ListPool<int>(poolSize);
        var list = listPool.Get();
        list.Add(1);
        list.Add(2);

        // Act
        listPool.Return(list);
        var reusedList = listPool.Get();

        // Assert
        Assert.NotNull(reusedList);
        Assert.Empty(reusedList);
    }

    [Fact]
    public void Get_ShouldReuseList_WhenReturnedToPool()
    {
        // Arrange
        int poolSize = 10;
        var listPool = new ListPool<int>(poolSize);

        // Act
        var list1 = listPool.Get();
        listPool.Return(list1);
        var list2 = listPool.Get();

        // Assert
        Assert.Same(list1, list2);
    }

    [Fact]
    public void Get_Disposable_ShouldReturnListAndDisposeProperly()
    {
        // Arrange
        int poolSize = 10;
        var listPool = new ListPool<int>(poolSize);

        // Act
        using (listPool.Get(out List<int> list))
        {
            list.Add(42);
            Assert.Single(list);
        }

        // Assert
        var reusedList = listPool.Get();
        Assert.Empty(reusedList);
    }

    [Fact]
    public void Dispose_ShouldClearObjectPool()
    {
        // Arrange
        int poolSize = 10;
        var listPool = new ListPool<int>(poolSize);

        var list1 = listPool.Get();
        list1.Add(42);
        listPool.Return(list1);

        // Act
        listPool.Dispose();

        // Assert
        var list2 = listPool.Get();
        Assert.NotNull(list2);
        Assert.Empty(list2);
        Assert.NotSame(list1, list2);
    }
}