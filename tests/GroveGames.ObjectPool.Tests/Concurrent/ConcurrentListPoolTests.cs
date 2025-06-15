using System.Collections.Concurrent;

using GroveGames.ObjectPool.Concurrent;

namespace GroveGames.ObjectPool.Tests.Concurrent;

public sealed class ConcurrentListPoolTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesPool()
    {
        // Arrange & Act
        using var pool = new ConcurrentListPool<string>(5, 10);

        // Assert
        Assert.Equal(10, pool.MaxSize);
    }

    [Fact]
    public void Constructor_NegativeInitialSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentListPool<string>(-1, 10));
    }

    [Fact]
    public void Constructor_InitialSizeGreaterThanMaxSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentListPool<string>(15, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidMaxSize_ThrowsArgumentOutOfRangeException(int maxSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentListPool<string>(5, maxSize));
    }

    [Fact]
    public void Rent_ValidPool_ReturnsList()
    {
        // Arrange
        using var pool = new ConcurrentListPool<string>(1, 5);

        // Act
        var list = pool.Rent();

        // Assert
        Assert.NotNull(list);
        Assert.IsType<List<string>>(list);
    }

    [Fact]
    public void Rent_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentListPool<string>(1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Rent());
    }

    [Fact]
    public void Return_ValidList_DoesNotThrow()
    {
        // Arrange
        using var pool = new ConcurrentListPool<string>(1, 5);
        var list = pool.Rent();

        // Act & Assert
        pool.Return(list);
    }

    [Fact]
    public void Return_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentListPool<string>(1, 5);
        var list = pool.Rent();
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Return(list));
    }

    [Fact]
    public void Clear_ValidPool_DoesNotThrow()
    {
        // Arrange
        using var pool = new ConcurrentListPool<string>(1, 5);

        // Act & Assert
        pool.Clear();
    }

    [Fact]
    public void Clear_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentListPool<string>(1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Clear());
    }

    [Fact]
    public void Count_ValidPool_ReturnsValue()
    {
        // Arrange
        using var pool = new ConcurrentListPool<string>(1, 5);

        // Act
        var count = pool.Count;

        // Assert
        Assert.True(count >= 0);
    }

    [Fact]
    public void Count_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentListPool<string>(1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Count);
    }

    [Fact]
    public void MaxSize_ValidPool_ReturnsExpectedValue()
    {
        // Arrange
        using var pool = new ConcurrentListPool<string>(1, 10);

        // Act
        var maxSize = pool.MaxSize;

        // Assert
        Assert.Equal(10, maxSize);
    }

    [Fact]
    public void MaxSize_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentListPool<string>(1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.MaxSize);
    }

    [Fact]
    public void Dispose_CalledOnce_DoesNotThrow()
    {
        // Arrange
        var pool = new ConcurrentListPool<string>(1, 5);

        // Act & Assert
        pool.Dispose();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var pool = new ConcurrentListPool<string>(1, 5);

        // Act & Assert
        pool.Dispose();
        pool.Dispose();
        pool.Dispose();
    }

    [Fact]
    public void RentReturn_ListIsCleared_AfterReturn()
    {
        // Arrange
        using var pool = new ConcurrentListPool<string>(1, 5);
        var list1 = pool.Rent();
        list1.Add("item");
        pool.Return(list1);

        // Act
        var list2 = pool.Rent();

        // Assert
        Assert.Empty(list2);
        pool.Return(list2);
    }

    [Fact]
    public void RentReturn_ListOperations_WorkCorrectly()
    {
        // Arrange
        using var pool = new ConcurrentListPool<int>(1, 5);
        var list = pool.Rent();

        // Act
        list.Add(1);
        list.Add(2);
        list.Insert(1, 3);

        // Assert
        Assert.Equal(3, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(3, list[1]);
        Assert.Equal(2, list[2]);

        pool.Return(list);
    }

    [Fact]
    public void RentReturn_ListMaintainsOrder_WithDuplicates()
    {
        // Arrange
        using var pool = new ConcurrentListPool<string>(1, 5);
        var list = pool.Rent();

        // Act
        list.Add("first");
        list.Add("second");
        list.Add("first");
        list.Add("third");

        // Assert
        Assert.Equal(4, list.Count);
        Assert.Equal("first", list[0]);
        Assert.Equal("second", list[1]);
        Assert.Equal("first", list[2]);
        Assert.Equal("third", list[3]);

        pool.Return(list);
    }

    [Fact]
    public void RentReturn_EmptyList_RemainsEmpty()
    {
        // Arrange
        using var pool = new ConcurrentListPool<string>(1, 5);

        // Act
        var list = pool.Rent();

        // Assert
        Assert.Empty(list);
        Assert.Equal(0, list.Count);

        pool.Return(list);
    }

    [Fact]
    public async Task Rent_ConcurrentAccess_AllSucceed()
    {
        // Arrange
        using var pool = new ConcurrentListPool<int>(10, 50);
        var results = new ConcurrentBag<List<int>>();

        // Act
        var tasks = Enumerable.Range(0, 20).Select(_ => Task.Run(() =>
        {
            var list = pool.Rent();
            results.Add(list);
            return list;
        })).ToArray();

        var lists = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(20, results.Count);
        Assert.All(lists, list => Assert.NotNull(list));

        foreach (var list in lists)
        {
            pool.Return(list);
        }
    }

    [Fact]
    public async Task RentReturn_ConcurrentOperations_AllSucceed()
    {
        // Arrange
        using var pool = new ConcurrentListPool<int>(5, 20);

        // Act
        var tasks = Enumerable.Range(0, 50).Select(i => Task.Run(() =>
        {
            var list = pool.Rent();
            list.Add(i);
            pool.Return(list);
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert - No exceptions thrown
    }

    [Fact]
    public async Task Dispose_ConcurrentWithOperations_HandlesGracefully()
    {
        // Arrange
        var pool = new ConcurrentListPool<int>(5, 20);
        var exceptions = new ConcurrentBag<Exception>();

        // Act
        var operationTasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            try
            {
                var list = pool.Rent();
                pool.Return(list);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        })).ToArray();

        var disposeTask = Task.Run(() =>
        {
            Thread.Sleep(10);
            pool.Dispose();
        });

        await Task.WhenAll(operationTasks.Concat(new[] { disposeTask }));

        // Assert - Some operations may throw ObjectDisposedException, which is expected
        Assert.All(exceptions, ex => Assert.IsType<ObjectDisposedException>(ex));
    }

    [Fact]
    public void RentReturn_ListIndexing_WorksCorrectly()
    {
        // Arrange
        using var pool = new ConcurrentListPool<string>(1, 5);
        var list = pool.Rent();

        // Act
        list.AddRange(new[] { "zero", "one", "two" });
        list[1] = "updated";

        // Assert
        Assert.Equal("zero", list[0]);
        Assert.Equal("updated", list[1]);
        Assert.Equal("two", list[2]);

        pool.Return(list);
    }

    [Fact]
    public void RentReturn_ListCapacity_HandlesGrowth()
    {
        // Arrange
        using var pool = new ConcurrentListPool<int>(1, 5);
        var list = pool.Rent();

        // Act
        for (int i = 0; i < 100; i++)
        {
            list.Add(i);
        }

        // Assert
        Assert.Equal(100, list.Count);
        Assert.True(list.Capacity >= 100);

        pool.Return(list);
    }
}