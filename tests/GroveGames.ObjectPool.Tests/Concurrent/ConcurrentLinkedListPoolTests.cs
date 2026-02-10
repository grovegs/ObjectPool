using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using GroveGames.ObjectPool.Concurrent;

namespace GroveGames.ObjectPool.Tests.Concurrent;

public sealed class ConcurrentLinkedListPoolTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesPool()
    {
        // Arrange & Act
        using var pool = new ConcurrentLinkedListPool<string>(5, 10);

        // Assert
        Assert.Equal(10, pool.MaxSize);
    }

    [Fact]
    public void Constructor_NegativeInitialSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentLinkedListPool<string>(-1, 10));
    }

    [Fact]
    public void Constructor_InitialSizeGreaterThanMaxSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentLinkedListPool<string>(15, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidMaxSize_ThrowsArgumentOutOfRangeException(int maxSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentLinkedListPool<string>(5, maxSize));
    }

    [Fact]
    public void Rent_ValidPool_ReturnsLinkedList()
    {
        // Arrange
        using var pool = new ConcurrentLinkedListPool<string>(1, 5);

        // Act
        var linkedList = pool.Rent();

        // Assert
        Assert.NotNull(linkedList);
        Assert.IsType<LinkedList<string>>(linkedList);
    }

    [Fact]
    public void Rent_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentLinkedListPool<string>(1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(pool.Rent);
    }

    [Fact]
    public void Return_ValidLinkedList_DoesNotThrow()
    {
        // Arrange
        using var pool = new ConcurrentLinkedListPool<string>(1, 5);
        var linkedList = pool.Rent();

        // Act & Assert
        pool.Return(linkedList);
    }

    [Fact]
    public void Return_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentLinkedListPool<string>(1, 5);
        var linkedList = pool.Rent();
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Return(linkedList));
    }

    [Fact]
    public void Clear_ValidPool_DoesNotThrow()
    {
        // Arrange
        using var pool = new ConcurrentLinkedListPool<string>(1, 5);

        // Act & Assert
        pool.Clear();
    }

    [Fact]
    public void Clear_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentLinkedListPool<string>(1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(pool.Clear);
    }

    [Fact]
    public void Count_ValidPool_ReturnsValue()
    {
        // Arrange
        using var pool = new ConcurrentLinkedListPool<string>(1, 5);

        // Act
        var count = pool.Count;

        // Assert
        Assert.True(count >= 0);
    }

    [Fact]
    public void Count_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentLinkedListPool<string>(1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Count);
    }

    [Fact]
    public void MaxSize_ValidPool_ReturnsExpectedValue()
    {
        // Arrange
        using var pool = new ConcurrentLinkedListPool<string>(1, 10);

        // Act
        var maxSize = pool.MaxSize;

        // Assert
        Assert.Equal(10, maxSize);
    }

    [Fact]
    public void MaxSize_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentLinkedListPool<string>(1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.MaxSize);
    }

    [Fact]
    public void Dispose_CalledOnce_DoesNotThrow()
    {
        // Arrange
        var pool = new ConcurrentLinkedListPool<string>(1, 5);

        // Act & Assert
        pool.Dispose();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var pool = new ConcurrentLinkedListPool<string>(1, 5);

        // Act & Assert
        pool.Dispose();
        pool.Dispose();
        pool.Dispose();
    }

    [Fact]
    public void RentReturn_LinkedListIsCleared_AfterReturn()
    {
        // Arrange
        using var pool = new ConcurrentLinkedListPool<string>(1, 5);
        var linkedList1 = pool.Rent();
        linkedList1.AddLast("item");
        pool.Return(linkedList1);

        // Act
        var linkedList2 = pool.Rent();

        // Assert
        Assert.Empty(linkedList2);
        pool.Return(linkedList2);
    }

    [Fact]
    public void RentReturn_LinkedListOperations_WorkCorrectly()
    {
        // Arrange
        using var pool = new ConcurrentLinkedListPool<int>(1, 5);
        var linkedList = pool.Rent();

        // Act
        linkedList.AddFirst(1);
        linkedList.AddLast(2);
        linkedList.AddAfter(linkedList.First!, 3);

        // Assert
        Assert.Equal(3, linkedList.Count);
        Assert.Equal(1, linkedList.First!.Value);
        Assert.Equal(2, linkedList.Last!.Value);
        Assert.Equal(3, linkedList.First.Next!.Value);

        pool.Return(linkedList);
    }

    [Fact]
    public async Task Rent_ConcurrentAccess_AllSucceed()
    {
        // Arrange
        using var pool = new ConcurrentLinkedListPool<int>(10, 50);
        var results = new ConcurrentBag<LinkedList<int>>();

        // Act
        var tasks = Enumerable.Range(0, 20).Select(_ => Task.Run(() =>
        {
            var linkedList = pool.Rent();
            results.Add(linkedList);
            return linkedList;
        })).ToArray();

        var linkedLists = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(20, results.Count);
        Assert.All(linkedLists, Assert.NotNull);

        foreach (var linkedList in linkedLists)
        {
            pool.Return(linkedList);
        }
    }

    [Fact]
    public async Task RentReturn_ConcurrentOperations_AllSucceed()
    {
        // Arrange
        using var pool = new ConcurrentLinkedListPool<int>(5, 20);

        // Act
        var tasks = Enumerable.Range(0, 50).Select(i => Task.Run(() =>
        {
            var linkedList = pool.Rent();
            linkedList.AddFirst(i);
            pool.Return(linkedList);
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert - No exceptions thrown
    }

    [Fact]
    public async Task Dispose_ConcurrentWithOperations_HandlesGracefully()
    {
        // Arrange
        var pool = new ConcurrentLinkedListPool<int>(5, 20);
        var exceptions = new ConcurrentBag<Exception>();

        // Act
        var operationTasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            try
            {
                var linkedList = pool.Rent();
                pool.Return(linkedList);
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
        }, TestContext.Current.CancellationToken);

        await Task.WhenAll(operationTasks.Concat([disposeTask]));

        // Assert
        Assert.All(exceptions, ex => Assert.IsType<ObjectDisposedException>(ex));
    }

    [Fact]
    public void RentReturn_MultipleItems_MaintainsOrder()
    {
        // Arrange
        using var pool = new ConcurrentLinkedListPool<string>(1, 5);
        var linkedList = pool.Rent();

        // Act
        linkedList.AddLast("first");
        linkedList.AddLast("second");
        linkedList.AddLast("third");

        // Assert
        var items = linkedList.ToArray();
        Assert.Equal(new[] { "first", "second", "third" }, items);

        pool.Return(linkedList);
    }

    [Fact]
    public void RentReturn_EmptyLinkedList_RemainsEmpty()
    {
        // Arrange
        using var pool = new ConcurrentLinkedListPool<string>(1, 5);

        // Act
        var linkedList = pool.Rent();

        // Assert
        Assert.Empty(linkedList);
        Assert.Null(linkedList.First);
        Assert.Null(linkedList.Last);

        pool.Return(linkedList);
    }
}
