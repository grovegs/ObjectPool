using System;
using System.Collections.Generic;

namespace GroveGames.ObjectPool.Tests;

public sealed class QueuePoolTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesPool()
    {
        // Arrange & Act
        using var pool = new QueuePool<int>(5, 10);

        // Assert
        Assert.Equal(0, pool.Count);
        Assert.Equal(10, pool.MaxSize);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_NegativeInitialSize_ThrowsArgumentOutOfRangeException(int initialSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new QueuePool<int>(initialSize, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_NonPositiveMaxSize_ThrowsArgumentOutOfRangeException(int maxSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new QueuePool<int>(5, maxSize));
    }

    [Fact]
    public void Constructor_InitialSizeGreaterThanMaxSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new QueuePool<int>(15, 10));
    }

    [Fact]
    public void Rent_EmptyPool_ReturnsNewQueue()
    {
        // Arrange
        using var pool = new QueuePool<string>(0, 5);

        // Act
        var queue = pool.Rent();

        // Assert
        Assert.NotNull(queue);
        Assert.Empty(queue);
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Return_Queue_AddsToPool()
    {
        // Arrange
        using var pool = new QueuePool<string>(0, 5);
        var queue = pool.Rent();

        // Act
        pool.Return(queue);

        // Assert
        Assert.Equal(1, pool.Count);
    }

    [Fact]
    public void Return_QueueWithData_ClearsQueue()
    {
        // Arrange
        using var pool = new QueuePool<int>(0, 5);
        var queue = pool.Rent();
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);

        // Act
        pool.Return(queue);
        var reusedQueue = pool.Rent();

        // Assert
        Assert.Same(queue, reusedQueue);
        Assert.Empty(reusedQueue);
    }

    [Fact]
    public void RentAndReturn_ReusesQueues()
    {
        // Arrange
        using var pool = new QueuePool<string>(0, 5);
        var originalQueue = pool.Rent();
        pool.Return(originalQueue);

        // Act
        var reusedQueue = pool.Rent();

        // Assert
        Assert.Same(originalQueue, reusedQueue);
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Clear_RemovesAllQueues()
    {
        // Arrange
        using var pool = new QueuePool<int>(0, 5);
        var queue1 = pool.Rent();
        var queue2 = pool.Rent();
        var queue3 = pool.Rent();
        pool.Return(queue1);
        pool.Return(queue2);
        pool.Return(queue3);

        // Act
        pool.Clear();

        // Assert
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Count_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new QueuePool<int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Count);
    }

    [Fact]
    public void MaxSize_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new QueuePool<int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.MaxSize);
    }

    [Fact]
    public void Rent_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new QueuePool<int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(pool.Rent);
    }

    [Fact]
    public void Return_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new QueuePool<int>(0, 5);
        var queue = new Queue<int>();
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Return(queue));
    }

    [Fact]
    public void Clear_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new QueuePool<int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(pool.Clear);
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        // Arrange
        var pool = new QueuePool<int>(0, 5);

        // Act & Assert
        pool.Dispose();
        pool.Dispose();
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 10)]
    [InlineData(0, 100)]
    public void Constructor_ValidSizeCombinations_CreatesPool(int initialSize, int maxSize)
    {
        // Arrange & Act
        using var pool = new QueuePool<int>(initialSize, maxSize);

        // Assert
        Assert.Equal(0, pool.Count);
        Assert.Equal(maxSize, pool.MaxSize);
    }

    [Fact]
    public void RentMultiple_EmptyPool_CreatesMultipleQueues()
    {
        // Arrange
        using var pool = new QueuePool<string>(0, 5);

        // Act
        var queue1 = pool.Rent();
        var queue2 = pool.Rent();
        var queue3 = pool.Rent();

        // Assert
        Assert.NotSame(queue1, queue2);
        Assert.NotSame(queue2, queue3);
        Assert.NotSame(queue1, queue3);
        Assert.Empty(queue1);
        Assert.Empty(queue2);
        Assert.Empty(queue3);
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Return_QueueWithFIFOOperations_ClearsAllItems()
    {
        // Arrange
        using var pool = new QueuePool<int>(0, 5);
        var queue = pool.Rent();
        queue.Enqueue(10);
        queue.Enqueue(20);
        queue.Enqueue(30);
        var first = queue.Dequeue();

        // Act
        pool.Return(queue);
        var reusedQueue = pool.Rent();

        // Assert
        Assert.Same(queue, reusedQueue);
        Assert.Empty(reusedQueue);
        Assert.Equal(10, first);
    }

    [Fact]
    public void Return_MaxSizeReached_DoesNotAddToPool()
    {
        // Arrange
        using var pool = new QueuePool<int>(0, 2);
        var queue1 = new Queue<int>();
        var queue2 = new Queue<int>();
        var queue3 = new Queue<int>();

        // Act
        pool.Return(queue1);
        pool.Return(queue2);
        pool.Return(queue3);

        // Assert
        Assert.Equal(2, pool.Count);
    }

    [Fact]
    public void Clear_EmptyPool_DoesNotThrow()
    {
        // Arrange
        using var pool = new QueuePool<int>(0, 5);

        // Act & Assert
        pool.Clear(); // Should not throw
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Return_QueueWithReferenceTypes_ClearsCorrectly()
    {
        // Arrange
        using var pool = new QueuePool<string>(0, 5);
        var queue = pool.Rent();
        queue.Enqueue("first");
        queue.Enqueue("second");
        queue.Enqueue("third");

        // Act
        pool.Return(queue);
        var reusedQueue = pool.Rent();

        // Assert
        Assert.Same(queue, reusedQueue);
        Assert.Empty(reusedQueue);
    }

    [Fact]
    public void Queue_AfterClear_BehavesLikeNewQueue()
    {
        // Arrange
        using var pool = new QueuePool<int>(0, 5);
        var queue = pool.Rent();
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        pool.Return(queue);

        // Act
        var reusedQueue = pool.Rent();
        reusedQueue.Enqueue(100);
        reusedQueue.Enqueue(200);

        // Assert
        Assert.Same(queue, reusedQueue);
        Assert.Equal(2, reusedQueue.Count);
        Assert.Equal(100, reusedQueue.Dequeue());
        Assert.Equal(200, reusedQueue.Dequeue());
    }

    [Fact]
    public void Pool_WithLargeDataSet_HandlesClearingCorrectly()
    {
        // Arrange
        using var pool = new QueuePool<int>(0, 5);
        var queue = pool.Rent();

        for (int i = 0; i < 1000; i++)
        {
            queue.Enqueue(i);
        }

        // Act
        pool.Return(queue);
        var reusedQueue = pool.Rent();

        // Assert
        Assert.Same(queue, reusedQueue);
        Assert.Empty(reusedQueue);
    }

    [Fact]
    public void Return_QueueWithComplexObjects_ClearsCorrectly()
    {
        // Arrange
        using var pool = new QueuePool<List<int>>(0, 5);
        var queue = pool.Rent();
        queue.Enqueue([1, 2, 3]);
        queue.Enqueue([4, 5, 6]);
        queue.Enqueue([7, 8, 9]);

        // Act
        pool.Return(queue);
        var reusedQueue = pool.Rent();

        // Assert
        Assert.Same(queue, reusedQueue);
        Assert.Empty(reusedQueue);
    }

    [Fact]
    public void Queue_FIFOBehavior_PreservedAfterClear()
    {
        // Arrange
        using var pool = new QueuePool<string>(0, 5);
        var queue = pool.Rent();
        queue.Enqueue("old1");
        queue.Enqueue("old2");
        pool.Return(queue);

        // Act
        var reusedQueue = pool.Rent();
        reusedQueue.Enqueue("new1");
        reusedQueue.Enqueue("new2");
        reusedQueue.Enqueue("new3");

        // Assert
        Assert.Same(queue, reusedQueue);
        Assert.Equal(3, reusedQueue.Count);
        Assert.Equal("new1", reusedQueue.Dequeue());
        Assert.Equal("new2", reusedQueue.Dequeue());
        Assert.Equal("new3", reusedQueue.Dequeue());
    }

    [Fact]
    public void Return_QueueAfterPartialDequeue_ClearsCorrectly()
    {
        // Arrange
        using var pool = new QueuePool<int>(0, 5);
        var queue = pool.Rent();
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        queue.Enqueue(4);
        queue.Dequeue();
        queue.Dequeue();

        // Act
        pool.Return(queue);
        var reusedQueue = pool.Rent();

        // Assert
        Assert.Same(queue, reusedQueue);
        Assert.Empty(reusedQueue);
    }

    [Fact]
    public void Pool_WithValueTypes_HandlesCorrectly()
    {
        // Arrange
        using var pool = new QueuePool<DateTime>(0, 5);
        var queue = pool.Rent();
        queue.Enqueue(DateTime.Now);
        queue.Enqueue(DateTime.UtcNow);
        queue.Enqueue(DateTime.Today);

        // Act
        pool.Return(queue);
        var reusedQueue = pool.Rent();

        // Assert
        Assert.Same(queue, reusedQueue);
        Assert.Empty(reusedQueue);
    }

    [Fact]
    public void Queue_PeekOperation_WorksCorrectlyAfterClear()
    {
        // Arrange
        using var pool = new QueuePool<string>(0, 5);
        var queue = pool.Rent();
        queue.Enqueue("old");
        pool.Return(queue);

        // Act
        var reusedQueue = pool.Rent();
        reusedQueue.Enqueue("new");

        // Assert
        Assert.Same(queue, reusedQueue);
        Assert.Equal("new", reusedQueue.Peek());
        Assert.Single(reusedQueue);
    }
}
