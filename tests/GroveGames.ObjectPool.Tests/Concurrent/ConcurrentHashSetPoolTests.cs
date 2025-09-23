using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using GroveGames.ObjectPool.Concurrent;

namespace GroveGames.ObjectPool.Tests.Concurrent;

public sealed class ConcurrentHashSetPoolTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesPool()
    {
        // Arrange & Act
        using var pool = new ConcurrentHashSetPool<string>(5, 10);

        // Assert
        Assert.Equal(10, pool.MaxSize);
    }

    [Fact]
    public void Constructor_WithComparer_CreatesPool()
    {
        // Arrange & Act
        using var pool = new ConcurrentHashSetPool<string>(5, 10, StringComparer.OrdinalIgnoreCase);

        // Assert
        Assert.Equal(10, pool.MaxSize);
    }

    [Fact]
    public void Constructor_NegativeInitialSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentHashSetPool<string>(-1, 10));
    }

    [Fact]
    public void Constructor_InitialSizeGreaterThanMaxSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentHashSetPool<string>(15, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidMaxSize_ThrowsArgumentOutOfRangeException(int maxSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentHashSetPool<string>(5, maxSize));
    }

    [Fact]
    public void Rent_ValidPool_ReturnsHashSet()
    {
        // Arrange
        using var pool = new ConcurrentHashSetPool<string>(1, 5);

        // Act
        var hashSet = pool.Rent();

        // Assert
        Assert.NotNull(hashSet);
        Assert.IsType<HashSet<string>>(hashSet);
    }

    [Fact]
    public void Rent_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentHashSetPool<string>(1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(pool.Rent);
    }

    [Fact]
    public void Return_ValidHashSet_DoesNotThrow()
    {
        // Arrange
        using var pool = new ConcurrentHashSetPool<string>(1, 5);
        var hashSet = pool.Rent();

        // Act & Assert
        pool.Return(hashSet);
    }

    [Fact]
    public void Return_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentHashSetPool<string>(1, 5);
        var hashSet = pool.Rent();
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Return(hashSet));
    }

    [Fact]
    public void Clear_ValidPool_DoesNotThrow()
    {
        // Arrange
        using var pool = new ConcurrentHashSetPool<string>(1, 5);

        // Act & Assert
        pool.Clear();
    }

    [Fact]
    public void Clear_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentHashSetPool<string>(1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(pool.Clear);
    }

    [Fact]
    public void Count_ValidPool_ReturnsValue()
    {
        // Arrange
        using var pool = new ConcurrentHashSetPool<string>(1, 5);

        // Act
        var count = pool.Count;

        // Assert
        Assert.True(count >= 0);
    }

    [Fact]
    public void Count_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentHashSetPool<string>(1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Count);
    }

    [Fact]
    public void MaxSize_ValidPool_ReturnsExpectedValue()
    {
        // Arrange
        using var pool = new ConcurrentHashSetPool<string>(1, 10);

        // Act
        var maxSize = pool.MaxSize;

        // Assert
        Assert.Equal(10, maxSize);
    }

    [Fact]
    public void MaxSize_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentHashSetPool<string>(1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.MaxSize);
    }

    [Fact]
    public void Dispose_CalledOnce_DoesNotThrow()
    {
        // Arrange
        var pool = new ConcurrentHashSetPool<string>(1, 5);

        // Act & Assert
        pool.Dispose();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var pool = new ConcurrentHashSetPool<string>(1, 5);

        // Act & Assert
        pool.Dispose();
        pool.Dispose();
        pool.Dispose();
    }

    [Fact]
    public void RentReturn_WithComparer_HashSetUsesComparer()
    {
        // Arrange
        using var pool = new ConcurrentHashSetPool<string>(1, 5, StringComparer.OrdinalIgnoreCase);
        var hashSet = pool.Rent();

        // Act
        hashSet.Add("Test");
        var containsLowercase = hashSet.Contains("test");
        pool.Return(hashSet);

        // Assert
        Assert.True(containsLowercase);
    }

    [Fact]
    public void RentReturn_HashSetIsCleared_AfterReturn()
    {
        // Arrange
        using var pool = new ConcurrentHashSetPool<string>(1, 5);
        var hashSet1 = pool.Rent();
        hashSet1.Add("item");
        pool.Return(hashSet1);

        // Act
        var hashSet2 = pool.Rent();

        // Assert
        Assert.Empty(hashSet2);
        pool.Return(hashSet2);
    }

    [Fact]
    public async Task Rent_ConcurrentAccess_AllSucceed()
    {
        // Arrange
        using var pool = new ConcurrentHashSetPool<int>(10, 50);
        var results = new ConcurrentBag<HashSet<int>>();

        // Act
        var tasks = Enumerable.Range(0, 20).Select(_ => Task.Run(() =>
        {
            var hashSet = pool.Rent();
            results.Add(hashSet);
            return hashSet;
        })).ToArray();

        var hashSets = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(20, results.Count);
        Assert.All(hashSets, Assert.NotNull);

        foreach (var hashSet in hashSets)
        {
            pool.Return(hashSet);
        }
    }

    [Fact]
    public async Task RentReturn_ConcurrentOperations_AllSucceed()
    {
        // Arrange
        using var pool = new ConcurrentHashSetPool<int>(5, 20);

        // Act
        var tasks = Enumerable.Range(0, 50).Select(i => Task.Run(() =>
        {
            var hashSet = pool.Rent();
            hashSet.Add(i);
            pool.Return(hashSet);
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert - No exceptions thrown
    }

    [Fact]
    public async Task Dispose_ConcurrentWithOperations_HandlesGracefully()
    {
        // Arrange
        var pool = new ConcurrentHashSetPool<int>(5, 20);
        var exceptions = new ConcurrentBag<Exception>();

        // Act
        var operationTasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            try
            {
                var hashSet = pool.Rent();
                pool.Return(hashSet);
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
}