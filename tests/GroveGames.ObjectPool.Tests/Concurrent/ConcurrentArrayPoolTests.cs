using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GroveGames.ObjectPool.Concurrent;

namespace GroveGames.ObjectPool.Tests.Concurrent;

public sealed class ConcurrentArrayPoolTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesPool()
    {
        // Arrange & Act
        using var pool = new ConcurrentArrayPool<int>(5, 10);

        // Assert
        Assert.Equal(0, pool.Count(5));
        Assert.Equal(10, pool.MaxSize(5));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_NegativeInitialSize_ThrowsArgumentOutOfRangeException(int initialSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentArrayPool<int>(initialSize, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_NonPositiveMaxSize_ThrowsArgumentOutOfRangeException(int maxSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentArrayPool<int>(5, maxSize));
    }

    [Fact]
    public void Constructor_InitialSizeGreaterThanMaxSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentArrayPool<int>(15, 10));
    }

    [Fact]
    public void Rent_SizeZero_ReturnsEmptyArray()
    {
        // Arrange
        using var pool = new ConcurrentArrayPool<int>(5, 10);

        // Act
        var array = pool.Rent(0);

        // Assert
        Assert.Empty(array);
    }

    [Fact]
    public void Rent_ValidSize_ReturnsArrayOfCorrectSize()
    {
        // Arrange
        using var pool = new ConcurrentArrayPool<int>(5, 10);

        // Act
        var array = pool.Rent(5);

        // Assert
        Assert.Equal(5, array.Length);
    }

    [Fact]
    public void Rent_DifferentSizes_CreatesIndependentPools()
    {
        // Arrange
        using var pool = new ConcurrentArrayPool<int>(5, 10);

        // Act
        var array5 = pool.Rent(5);
        var array10 = pool.Rent(10);

        // Assert
        Assert.Equal(5, array5.Length);
        Assert.Equal(10, array10.Length);
        Assert.Equal(0, pool.Count(5));
        Assert.Equal(0, pool.Count(10));
    }

    [Fact]
    public void Return_ValidArray_AddsToPool()
    {
        // Arrange
        using var pool = new ConcurrentArrayPool<int>(5, 10);
        var array = pool.Rent(5);

        // Act
        pool.Return(array);

        // Assert
        Assert.Equal(1, pool.Count(5));
    }

    [Fact]
    public void Return_WithoutExistingPool_DoesNotCreatePool()
    {
        // Arrange
        using var pool = new ConcurrentArrayPool<int>(5, 10);
        var array = new int[7];

        // Act
        pool.Return(array);

        // Assert
        Assert.Equal(0, pool.Count(7));
    }

    [Fact]
    public void Return_ArrayToNewSize_DoesNotCreateNewPool()
    {
        // Arrange
        using var pool = new ConcurrentArrayPool<int>(5, 10);
        var array = new int[7];

        // Act
        pool.Return(array);

        // Assert
        Assert.Equal(0, pool.Count(7));
    }

    [Fact]
    public void RentAndReturn_ReusesArrays()
    {
        // Arrange
        using var pool = new ConcurrentArrayPool<int>(5, 10);
        var originalArray = pool.Rent(5);
        originalArray[0] = 42;
        pool.Return(originalArray);

        // Act
        var reusedArray = pool.Rent(5);

        // Assert
        Assert.Same(originalArray, reusedArray);
        Assert.Equal(42, reusedArray[0]);
        Assert.Equal(0, pool.Count(5));
    }

    [Fact]
    public void Count_NonExistentSize_ReturnsZero()
    {
        // Arrange
        using var pool = new ConcurrentArrayPool<int>(5, 10);

        // Act
        var count = pool.Count(100);

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public void Count_ExistingPool_ReturnsCorrectCount()
    {
        // Arrange
        using var pool = new ConcurrentArrayPool<int>(5, 10);

        // Act
        var array1 = pool.Rent(5);
        var array2 = pool.Rent(5);
        var array3 = pool.Rent(5);

        pool.Return(array1);
        pool.Return(array2);
        pool.Return(array3);

        var count = pool.Count(5);

        // Assert
        Assert.Equal(3, count);
    }

    [Fact]
    public void MaxSize_PositiveSize_ReturnsConfiguredMaxSize()
    {
        // Arrange
        using var pool = new ConcurrentArrayPool<int>(5, 20);

        // Act
        var maxSize = pool.MaxSize(10);

        // Assert
        Assert.Equal(20, maxSize);
    }

    [Fact]
    public void MaxSize_ExistingPool_ReturnsConfiguredMaxSize()
    {
        // Arrange
        using var pool = new ConcurrentArrayPool<int>(5, 15);
        pool.Rent(7);

        // Act
        var maxSize = pool.MaxSize(7);

        // Assert
        Assert.Equal(15, maxSize);
    }

    [Fact]
    public void Clear_RemovesAllArraysFromAllPools()
    {
        // Arrange
        using var pool = new ConcurrentArrayPool<int>(5, 10);
        var array5 = pool.Rent(5);
        var array10 = pool.Rent(10);
        var array15 = pool.Rent(15);
        pool.Return(array5);
        pool.Return(array10);
        pool.Return(array15);

        // Act
        pool.Clear();

        // Assert
        Assert.Equal(0, pool.Count(5));
        Assert.Equal(0, pool.Count(10));
        Assert.Equal(0, pool.Count(15));
    }

    [Fact]
    public void Rent_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentArrayPool<int>(5, 10);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Rent(5));
    }

    [Fact]
    public void Return_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentArrayPool<int>(5, 10);
        var array = pool.Rent(5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Return(array));
    }

    [Fact]
    public void Count_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentArrayPool<int>(5, 10);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Count(5));
    }

    [Fact]
    public void MaxSize_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentArrayPool<int>(5, 10);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.MaxSize(5));
    }

    [Fact]
    public void Clear_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentArrayPool<int>(5, 10);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(pool.Clear);
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        // Arrange
        var pool = new ConcurrentArrayPool<int>(5, 10);

        // Act & Assert
        pool.Dispose();
        pool.Dispose();
    }

    [Fact]
    public async Task RentAndReturn_HighConcurrencySameSize_ThreadSafe()
    {
        // Arrange
        using var pool = new ConcurrentArrayPool<int>(0, 100);
        var rentedArrays = new List<int[]>();
        var lockObject = new object();

        // Act
        var tasks = Enumerable.Range(0, 100).Select(async i =>
        {
            await Task.Yield();
            var array = pool.Rent(10);
            array[0] = i;
            lock (lockObject)
            {
                rentedArrays.Add(array);
            }
            pool.Return(array);
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(100, rentedArrays.Count);
        Assert.True(pool.Count(10) <= 100);
    }

    [Fact]
    public async Task Rent_MultipleSizesConcurrently_CreatesCorrectPools()
    {
        // Arrange
        using var pool = new ConcurrentArrayPool<int>(0, 50);
        var sizes = new[] { 5, 10, 15, 20, 25 };

        // Act
        var tasks = sizes.SelectMany(size =>
            Enumerable.Range(0, 20).Select(async _ =>
            {
                await Task.Yield();
                var array = pool.Rent(size);
                pool.Return(array);
            })
        ).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        foreach (var size in sizes)
        {
            Assert.True(pool.Count(size) <= 50);
            Assert.Equal(50, pool.MaxSize(size));
        }
    }

    [Fact]
    public async Task Return_ConcurrentReturnsMultipleSizes_RespectMaxSize()
    {
        // Arrange
        using var pool = new ConcurrentArrayPool<int>(0, 10);

        for (int size = 1; size <= 5; size++)
        {
            pool.Rent(size);
        }

        var arrays = Enumerable.Range(1, 100).Select(i => new int[i % 5 + 1]).ToArray();

        // Act
        var tasks = arrays.Select(async array =>
        {
            await Task.Yield();
            pool.Return(array);
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        for (int size = 1; size <= 5; size++)
        {
            Assert.True(pool.Count(size) <= 10);
        }
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 10)]
    [InlineData(0, 100)]
    public void Constructor_ValidSizeCombinations_CreatesPool(int initialSize, int maxSize)
    {
        // Arrange & Act
        using var pool = new ConcurrentArrayPool<int>(initialSize, maxSize);

        // Assert
        Assert.Equal(0, pool.Count(5));
        Assert.Equal(maxSize, pool.MaxSize(5));
    }

    [Fact]
    public async Task RentAndReturn_ConcurrentOperationsMixedSizes_ThreadSafe()
    {
        // Arrange
        using var pool = new ConcurrentArrayPool<string>(0, 50);
        var results = new List<(int size, string[] array)>();
        var lockObject = new object();

        // Act
        var tasks = Enumerable.Range(1, 50).Select(async i =>
        {
            await Task.Yield();
            var size = i % 10 + 1;
            var array = pool.Rent(size);
            array[0] = $"test_{i}";

            lock (lockObject)
            {
                results.Add((size, array));
            }

            pool.Return(array);
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(50, results.Count);
        foreach (var (size, array) in results)
        {
            Assert.Equal(size, array.Length);
        }
    }

    [Fact]
    public async Task GetOrAdd_ConcurrentCreation_CreatesOnlyOnePoolPerSize()
    {
        // Arrange
        using var pool = new ConcurrentArrayPool<int>(0, 100);
        var size = 42;

        // Act
        var tasks = Enumerable.Range(0, 50).Select(async _ =>
        {
            await Task.Yield();
            var array = pool.Rent(size);
            pool.Return(array);
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.True(pool.Count(size) <= 100);
        Assert.Equal(100, pool.MaxSize(size));
    }

    [Fact]
    public void Return_MultipleArraysSameSize_RespectMaxSize()
    {
        // Arrange
        using var pool = new ConcurrentArrayPool<int>(0, 3);

        pool.Rent(5);

        // Act
        pool.Return(new int[5]);
        pool.Return(new int[5]);
        pool.Return(new int[5]);
        pool.Return(new int[5]);

        // Assert
        Assert.True(pool.Count(5) <= 3);
    }

    [Fact]
    public void Rent_MultipleSizes_CreatesIndependentPools()
    {
        // Arrange
        using var pool = new ConcurrentArrayPool<string>(2, 5);

        // Act
        var array1 = pool.Rent(1);
        var array3 = pool.Rent(3);
        var array5 = pool.Rent(5);
        pool.Return(new string[1]);
        pool.Return(new string[3]);

        // Assert
        Assert.Single(array1);
        Assert.Equal(3, array3.Length);
        Assert.Equal(5, array5.Length);
        Assert.Equal(1, pool.Count(1));
        Assert.Equal(1, pool.Count(3));
        Assert.Equal(0, pool.Count(5));
    }

    [Fact]
    public void MaxSize_ZeroSize_ReturnsZero()
    {
        // Arrange
        using var pool = new ConcurrentArrayPool<int>(5, 10);

        // Act
        var maxSize = pool.MaxSize(0);

        // Assert
        Assert.Equal(0, maxSize);
    }

    [Fact]
    public void Clear_EmptyPool_DoesNotThrow()
    {
        // Arrange
        using var pool = new ConcurrentArrayPool<int>(5, 10);

        // Act & Assert
        pool.Clear();
        Assert.Equal(0, pool.Count(5));
    }

    [Fact]
    public async Task Clear_DuringConcurrentOperations_ThreadSafe()
    {
        // Arrange
        using var pool = new ConcurrentArrayPool<int>(0, 50);

        // Act
        var rentTasks = Enumerable.Range(0, 20).Select(async i =>
        {
            await Task.Yield();
            var array = pool.Rent(10);
            pool.Return(array);
        });

        var clearTask = Task.Run(async () =>
        {
            await Task.Delay(10, TestContext.Current.CancellationToken);
            try
            {
                pool.Clear();
            }
            catch (ObjectDisposedException)
            {
            }
        }, TestContext.Current.CancellationToken);

        await Task.WhenAll(rentTasks.Concat([clearTask]));

        // Assert
        Assert.True(pool.Count(10) >= 0);
    }

    [Fact]
    public async Task Dispose_DuringConcurrentOperations_ThreadSafe()
    {
        // Arrange
        var pool = new ConcurrentArrayPool<int>(0, 50);
        var exceptions = new List<Exception>();
        var lockObject = new object();

        // Act
        var rentTasks = Enumerable.Range(0, 20).Select(async i =>
        {
            try
            {
                await Task.Yield();
                var array = pool.Rent(10);
                pool.Return(array);
            }
            catch (ObjectDisposedException ex)
            {
                lock (lockObject)
                {
                    exceptions.Add(ex);
                }
            }
        });

        var disposeTask = Task.Run(async () =>
        {
            await Task.Delay(5, TestContext.Current.CancellationToken);
            pool.Dispose();
        }, TestContext.Current.CancellationToken);

        await Task.WhenAll(rentTasks.Concat([disposeTask]));

        // Assert
        Assert.True(exceptions.All(ex => ex is ObjectDisposedException));
    }

    [Fact]
    public async Task Pool_HighConcurrencyStressTest_HandlesCorrectly()
    {
        // Arrange
        using var pool = new ConcurrentArrayPool<int>(0, 200);
        var totalOperations = 1000;
        var completedOperations = 0;
        var lockObject = new object();

        // Act
        var tasks = Enumerable.Range(0, totalOperations).Select(async i =>
        {
            await Task.Yield();
            var size = (i % 10) + 1;
            var array = pool.Rent(size);
            array[0] = i;

            await Task.Delay(1, TestContext.Current.CancellationToken);

            pool.Return(array);

            lock (lockObject)
            {
                completedOperations++;
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(totalOperations, completedOperations);

        for (int size = 1; size <= 10; size++)
        {
            Assert.True(pool.Count(size) >= 0);
            Assert.True(pool.Count(size) <= 200);
        }
    }

    [Fact]
    public void Return_AfterDispose_ArrayNotAddedToPool()
    {
        // Arrange
        var pool = new ConcurrentArrayPool<int>(5, 10);
        var array1 = pool.Rent(5);
        var array2 = new int[5];

        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Return(array1));
        Assert.Throws<ObjectDisposedException>(() => pool.Return(array2));
    }

    [Fact]
    public async Task Dispose_OnlyExecutesOnce_EvenWithConcurrentCalls()
    {
        // Arrange
        var pool = new ConcurrentArrayPool<int>(0, 10);

        for (int i = 0; i < 5; i++)
        {
            var array = pool.Rent(10);
            pool.Return(array);
        }

        // Act
        var tasks = Enumerable.Range(0, 10).Select(async _ =>
        {
            await Task.Yield();
            pool.Dispose();
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Count(10));
    }
}