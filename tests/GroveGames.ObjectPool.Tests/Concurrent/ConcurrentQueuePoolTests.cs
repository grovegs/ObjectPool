using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using GroveGames.ObjectPool.Concurrent;

namespace GroveGames.ObjectPool.Tests.Concurrent;

public sealed class ConcurrentQueuePoolTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesPool()
    {
        // Arrange & Act
        using var pool = new ConcurrentQueuePool<string>(5, 10);

        // Assert
        Assert.Equal(10, pool.MaxSize);
    }

    [Fact]
    public void Constructor_NegativeInitialSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentQueuePool<string>(-1, 10));
    }

    [Fact]
    public void Constructor_InitialSizeGreaterThanMaxSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentQueuePool<string>(15, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidMaxSize_ThrowsArgumentOutOfRangeException(int maxSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentQueuePool<string>(5, maxSize));
    }

    [Fact]
    public void Rent_ValidPool_ReturnsQueue()
    {
        // Arrange
        using var pool = new ConcurrentQueuePool<string>(1, 5);

        // Act
        var queue = pool.Rent();

        // Assert
        Assert.NotNull(queue);
        Assert.IsType<Queue<string>>(queue);
    }

    [Fact]
    public void Rent_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentQueuePool<string>(1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Rent());
    }

    [Fact]
    public void Return_ValidQueue_DoesNotThrow()
    {
        // Arrange
        using var pool = new ConcurrentQueuePool<string>(1, 5);
        var queue = pool.Rent();

        // Act & Assert
        pool.Return(queue);
    }

    [Fact]
    public void Return_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentQueuePool<string>(1, 5);
        var queue = pool.Rent();
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Return(queue));
    }

    [Fact]
    public void Clear_ValidPool_DoesNotThrow()
    {
        // Arrange
        using var pool = new ConcurrentQueuePool<string>(1, 5);

        // Act & Assert
        pool.Clear();
    }

    [Fact]
    public void Clear_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentQueuePool<string>(1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Clear());
    }

    [Fact]
    public void Count_ValidPool_ReturnsValue()
    {
        // Arrange
        using var pool = new ConcurrentQueuePool<string>(1, 5);

        // Act
        var count = pool.Count;

        // Assert
        Assert.True(count >= 0);
    }

    [Fact]
    public void Count_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentQueuePool<string>(1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Count);
    }

    [Fact]
    public void MaxSize_ValidPool_ReturnsExpectedValue()
    {
        // Arrange
        using var pool = new ConcurrentQueuePool<string>(1, 10);

        // Act
        var maxSize = pool.MaxSize;

        // Assert
        Assert.Equal(10, maxSize);
    }

    [Fact]
    public void MaxSize_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentQueuePool<string>(1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.MaxSize);
    }

    [Fact]
    public void Dispose_CalledOnce_DoesNotThrow()
    {
        // Arrange
        var pool = new ConcurrentQueuePool<string>(1, 5);

        // Act & Assert
        pool.Dispose();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var pool = new ConcurrentQueuePool<string>(1, 5);

        // Act & Assert
        pool.Dispose();
        pool.Dispose();
        pool.Dispose();
    }

    [Fact]
    public void RentReturn_QueueIsCleared_AfterReturn()
    {
        // Arrange
        using var pool = new ConcurrentQueuePool<string>(1, 5);
        var queue1 = pool.Rent();
        queue1.Enqueue("item");
        pool.Return(queue1);

        // Act
        var queue2 = pool.Rent();

        // Assert
        Assert.Empty(queue2);
        pool.Return(queue2);
    }

    [Fact]
    public void RentReturn_QueueOperations_WorkCorrectly()
    {
        // Arrange
        using var pool = new ConcurrentQueuePool<int>(1, 5);
        var queue = pool.Rent();

        // Act
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);

        // Assert
        Assert.Equal(3, queue.Count);
        Assert.Equal(1, queue.Peek());
        Assert.Equal(1, queue.Dequeue());
        Assert.Equal(2, queue.Count);

        pool.Return(queue);
    }

    [Fact]
    public void RentReturn_QueueMaintainsFIFOOrder()
    {
        // Arrange
        using var pool = new ConcurrentQueuePool<string>(1, 5);
        var queue = pool.Rent();

        // Act
        queue.Enqueue("first");
        queue.Enqueue("second");
        queue.Enqueue("third");

        // Assert
        Assert.Equal("first", queue.Dequeue());
        Assert.Equal("second", queue.Dequeue());
        Assert.Equal("third", queue.Dequeue());

        pool.Return(queue);
    }

    [Fact]
    public void RentReturn_EmptyQueue_RemainsEmpty()
    {
        // Arrange
        using var pool = new ConcurrentQueuePool<string>(1, 5);

        // Act
        var queue = pool.Rent();

        // Assert
        Assert.Empty(queue);

        pool.Return(queue);
    }

    [Fact]
    public async Task Rent_ConcurrentAccess_AllSucceed()
    {
        // Arrange
        using var pool = new ConcurrentQueuePool<int>(10, 50);
        var results = new ConcurrentBag<Queue<int>>();

        // Act
        var tasks = Enumerable.Range(0, 20).Select(_ => Task.Run(() =>
        {
            var queue = pool.Rent();
            results.Add(queue);
            return queue;
        })).ToArray();

        var queues = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(20, results.Count);
        Assert.All(queues, queue => Assert.NotNull(queue));

        foreach (var queue in queues)
        {
            pool.Return(queue);
        }
    }

    [Fact]
    public async Task RentReturn_ConcurrentOperations_AllSucceed()
    {
        // Arrange
        using var pool = new ConcurrentQueuePool<int>(5, 20);

        // Act
        var tasks = Enumerable.Range(0, 50).Select(i => Task.Run(() =>
        {
            var queue = pool.Rent();
            queue.Enqueue(i);
            pool.Return(queue);
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert - No exceptions thrown
    }

    [Fact]
    public async Task Dispose_ConcurrentWithOperations_HandlesGracefully()
    {
        // Arrange
        var pool = new ConcurrentQueuePool<int>(5, 20);
        var exceptions = new ConcurrentBag<Exception>();

        // Act
        var operationTasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            try
            {
                var queue = pool.Rent();
                pool.Return(queue);
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

        // Assert - Some operations may throw ObjectDisposedException, which is expected
        Assert.All(exceptions, ex => Assert.IsType<ObjectDisposedException>(ex));
    }

    [Fact]
    public void RentReturn_QueueWithDuplicates_AllowsDuplicates()
    {
        // Arrange
        using var pool = new ConcurrentQueuePool<string>(1, 5);
        var queue = pool.Rent();

        // Act
        queue.Enqueue("duplicate");
        queue.Enqueue("unique");
        queue.Enqueue("duplicate");

        // Assert
        Assert.Equal(3, queue.Count);
        Assert.Equal("duplicate", queue.Dequeue());
        Assert.Equal("unique", queue.Dequeue());
        Assert.Equal("duplicate", queue.Dequeue());

        pool.Return(queue);
    }

    [Fact]
    public void RentReturn_QueueCapacity_HandlesLargeNumberOfItems()
    {
        // Arrange
        using var pool = new ConcurrentQueuePool<int>(1, 5);
        var queue = pool.Rent();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            queue.Enqueue(i);
        }

        // Assert
        Assert.Equal(1000, queue.Count);
        Assert.Equal(0, queue.Peek());

        pool.Return(queue);
    }

    [Fact]
    public void RentReturn_QueueTryMethods_WorkCorrectly()
    {
        // Arrange
        using var pool = new ConcurrentQueuePool<string>(1, 5);
        var queue = pool.Rent();

        // Act & Assert - Empty queue
        Assert.False(queue.TryPeek(out var peekResult));
        Assert.Null(peekResult);
        Assert.False(queue.TryDequeue(out var dequeueResult));
        Assert.Null(dequeueResult);

        // Act & Assert - Non-empty queue
        queue.Enqueue("test");
        Assert.True(queue.TryPeek(out var peekResult2));
        Assert.Equal("test", peekResult2);
        Assert.True(queue.TryDequeue(out var dequeueResult2));
        Assert.Equal("test", dequeueResult2);

        pool.Return(queue);
    }

    [Fact]
    public void RentReturn_QueueEnumerator_WorksCorrectly()
    {
        // Arrange
        using var pool = new ConcurrentQueuePool<string>(1, 5);
        var queue = pool.Rent();

        // Act
        queue.Enqueue("first");
        queue.Enqueue("second");
        queue.Enqueue("third");

        // Assert
        var items = queue.ToArray();
        Assert.Equal(new[] { "first", "second", "third" }, items);

        pool.Return(queue);
    }
}
