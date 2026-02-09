using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using GroveGames.ObjectPool.Concurrent;

namespace GroveGames.ObjectPool.Tests.Concurrent;

public sealed class ConcurrentStackPoolTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesPool()
    {
        // Arrange & Act
        using var pool = new ConcurrentStackPool<string>(5, 10);

        // Assert
        Assert.Equal(10, pool.MaxSize);
    }

    [Fact]
    public void Constructor_NegativeInitialSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentStackPool<string>(-1, 10));
    }

    [Fact]
    public void Constructor_InitialSizeGreaterThanMaxSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentStackPool<string>(15, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidMaxSize_ThrowsArgumentOutOfRangeException(int maxSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentStackPool<string>(5, maxSize));
    }

    [Fact]
    public void Rent_ValidPool_ReturnsStack()
    {
        // Arrange
        using var pool = new ConcurrentStackPool<string>(1, 5);

        // Act
        var stack = pool.Rent();

        // Assert
        Assert.NotNull(stack);
        Assert.IsType<Stack<string>>(stack);
    }

    [Fact]
    public void Rent_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentStackPool<string>(1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Rent());
    }

    [Fact]
    public void Return_ValidStack_DoesNotThrow()
    {
        // Arrange
        using var pool = new ConcurrentStackPool<string>(1, 5);
        var stack = pool.Rent();

        // Act & Assert
        pool.Return(stack);
    }

    [Fact]
    public void Return_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentStackPool<string>(1, 5);
        var stack = pool.Rent();
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Return(stack));
    }

    [Fact]
    public void Clear_ValidPool_DoesNotThrow()
    {
        // Arrange
        using var pool = new ConcurrentStackPool<string>(1, 5);

        // Act & Assert
        pool.Clear();
    }

    [Fact]
    public void Clear_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentStackPool<string>(1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Clear());
    }

    [Fact]
    public void Count_ValidPool_ReturnsValue()
    {
        // Arrange
        using var pool = new ConcurrentStackPool<string>(1, 5);

        // Act
        var count = pool.Count;

        // Assert
        Assert.True(count >= 0);
    }

    [Fact]
    public void Count_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentStackPool<string>(1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Count);
    }

    [Fact]
    public void MaxSize_ValidPool_ReturnsExpectedValue()
    {
        // Arrange
        using var pool = new ConcurrentStackPool<string>(1, 10);

        // Act
        var maxSize = pool.MaxSize;

        // Assert
        Assert.Equal(10, maxSize);
    }

    [Fact]
    public void MaxSize_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentStackPool<string>(1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.MaxSize);
    }

    [Fact]
    public void Dispose_CalledOnce_DoesNotThrow()
    {
        // Arrange
        var pool = new ConcurrentStackPool<string>(1, 5);

        // Act & Assert
        pool.Dispose();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var pool = new ConcurrentStackPool<string>(1, 5);

        // Act & Assert
        pool.Dispose();
        pool.Dispose();
        pool.Dispose();
    }

    [Fact]
    public void RentReturn_StackIsCleared_AfterReturn()
    {
        // Arrange
        using var pool = new ConcurrentStackPool<string>(1, 5);
        var stack1 = pool.Rent();
        stack1.Push("item");
        pool.Return(stack1);

        // Act
        var stack2 = pool.Rent();

        // Assert
        Assert.Empty(stack2);
        pool.Return(stack2);
    }

    [Fact]
    public void RentReturn_StackOperations_WorkCorrectly()
    {
        // Arrange
        using var pool = new ConcurrentStackPool<int>(1, 5);
        var stack = pool.Rent();

        // Act
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);

        // Assert
        Assert.Equal(3, stack.Count);
        Assert.Equal(3, stack.Peek());
        Assert.Equal(3, stack.Pop());
        Assert.Equal(2, stack.Count);

        pool.Return(stack);
    }

    [Fact]
    public void RentReturn_StackMaintainsLIFOOrder()
    {
        // Arrange
        using var pool = new ConcurrentStackPool<string>(1, 5);
        var stack = pool.Rent();

        // Act
        stack.Push("first");
        stack.Push("second");
        stack.Push("third");

        // Assert
        Assert.Equal("third", stack.Pop());
        Assert.Equal("second", stack.Pop());
        Assert.Equal("first", stack.Pop());

        pool.Return(stack);
    }

    [Fact]
    public void RentReturn_EmptyStack_RemainsEmpty()
    {
        // Arrange
        using var pool = new ConcurrentStackPool<string>(1, 5);

        // Act
        var stack = pool.Rent();

        // Assert
        Assert.Empty(stack);

        pool.Return(stack);
    }

    [Fact]
    public void RentReturn_StackWithValueTypes_WorksCorrectly()
    {
        // Arrange
        using var pool = new ConcurrentStackPool<int>(1, 5);
        var stack = pool.Rent();

        // Act
        stack.Push(42);
        stack.Push(100);

        // Assert
        Assert.Equal(2, stack.Count);
        Assert.Equal(100, stack.Pop());
        Assert.Equal(42, stack.Pop());

        pool.Return(stack);
    }

    [Fact]
    public async Task Rent_ConcurrentAccess_AllSucceed()
    {
        // Arrange
        using var pool = new ConcurrentStackPool<int>(10, 50);
        var results = new ConcurrentBag<Stack<int>>();

        // Act
        var tasks = Enumerable.Range(0, 20).Select(_ => Task.Run(() =>
        {
            var stack = pool.Rent();
            results.Add(stack);
            return stack;
        })).ToArray();

        var stacks = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(20, results.Count);
        Assert.All(stacks, stack => Assert.NotNull(stack));

        foreach (var stack in stacks)
        {
            pool.Return(stack);
        }
    }

    [Fact]
    public async Task RentReturn_ConcurrentOperations_AllSucceed()
    {
        // Arrange
        using var pool = new ConcurrentStackPool<int>(5, 20);

        // Act
        var tasks = Enumerable.Range(0, 50).Select(i => Task.Run(() =>
        {
            var stack = pool.Rent();
            stack.Push(i);
            pool.Return(stack);
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert - No exceptions thrown
    }

    [Fact]
    public async Task Dispose_ConcurrentWithOperations_HandlesGracefully()
    {
        // Arrange
        var pool = new ConcurrentStackPool<int>(5, 20);
        var exceptions = new ConcurrentBag<Exception>();

        // Act
        var operationTasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            try
            {
                var stack = pool.Rent();
                pool.Return(stack);
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
    public void RentReturn_StackWithDuplicates_AllowsDuplicates()
    {
        // Arrange
        using var pool = new ConcurrentStackPool<string>(1, 5);
        var stack = pool.Rent();

        // Act
        stack.Push("duplicate");
        stack.Push("unique");
        stack.Push("duplicate");

        // Assert
        Assert.Equal(3, stack.Count);
        Assert.Equal("duplicate", stack.Pop());
        Assert.Equal("unique", stack.Pop());
        Assert.Equal("duplicate", stack.Pop());

        pool.Return(stack);
    }

    [Fact]
    public void RentReturn_StackCapacity_HandlesLargeNumberOfItems()
    {
        // Arrange
        using var pool = new ConcurrentStackPool<int>(1, 5);
        var stack = pool.Rent();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            stack.Push(i);
        }

        // Assert
        Assert.Equal(1000, stack.Count);
        Assert.Equal(999, stack.Peek());

        pool.Return(stack);
    }

    [Fact]
    public void RentReturn_StackTryMethods_WorkCorrectly()
    {
        // Arrange
        using var pool = new ConcurrentStackPool<string>(1, 5);
        var stack = pool.Rent();

        // Act & Assert - Empty stack
        Assert.False(stack.TryPeek(out var peekResult));
        Assert.Null(peekResult);
        Assert.False(stack.TryPop(out var popResult));
        Assert.Null(popResult);

        // Act & Assert - Non-empty stack
        stack.Push("test");
        Assert.True(stack.TryPeek(out var peekResult2));
        Assert.Equal("test", peekResult2);
        Assert.True(stack.TryPop(out var popResult2));
        Assert.Equal("test", popResult2);

        pool.Return(stack);
    }

    [Fact]
    public void RentReturn_StackEnumerator_WorksCorrectly()
    {
        // Arrange
        using var pool = new ConcurrentStackPool<string>(1, 5);
        var stack = pool.Rent();

        // Act
        stack.Push("first");
        stack.Push("second");
        stack.Push("third");

        // Assert
        var items = stack.ToArray();
        Assert.Equal(new[] { "third", "second", "first" }, items);

        pool.Return(stack);
    }

    [Fact]
    public void RentReturn_StackContains_WorksCorrectly()
    {
        // Arrange
        using var pool = new ConcurrentStackPool<string>(1, 5);
        var stack = pool.Rent();

        // Act
        stack.Push("apple");
        stack.Push("banana");
        stack.Push("cherry");

        // Assert
        Assert.True(stack.Contains("banana"));
        Assert.False(stack.Contains("orange"));

        pool.Return(stack);
    }

    [Fact]
    public void RentReturn_StackCopyTo_WorksCorrectly()
    {
        // Arrange
        using var pool = new ConcurrentStackPool<int>(1, 5);
        var stack = pool.Rent();
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);

        // Act
        var array = new int[3];
        stack.CopyTo(array, 0);

        // Assert
        Assert.Equal([3, 2, 1], array);

        pool.Return(stack);
    }
}
