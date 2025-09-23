using System;
using System.Collections.Generic;

namespace GroveGames.ObjectPool.Tests;

public sealed class ObjectPoolTests
{
    private sealed class TestDisposable : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    [Fact]
    public void Constructor_ValidParameters_CreatesPool()
    {
        // Arrange & Act
        using var pool = new ObjectPool<string>(() => "test", null, null, 5, 10);

        // Assert
        Assert.Equal(0, pool.Count);
        Assert.Equal(10, pool.MaxSize);
    }

    [Fact]
    public void Constructor_NullFactory_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ObjectPool<string>(null!, null, null, 5, 10));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_NegativeInitialSize_ThrowsArgumentOutOfRangeException(int initialSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ObjectPool<string>(() => "test", null, null, initialSize, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_NonPositiveMaxSize_ThrowsArgumentOutOfRangeException(int maxSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ObjectPool<string>(() => "test", null, null, 5, maxSize));
    }

    [Fact]
    public void Constructor_InitialSizeGreaterThanMaxSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ObjectPool<string>(() => "test", null, null, 15, 10));
    }

    [Fact]
    public void Rent_EmptyPool_CreatesNewItem()
    {
        // Arrange
        using var pool = new ObjectPool<string>(() => "created", null, null, 0, 5);

        // Act
        var item = pool.Rent();

        // Assert
        Assert.Equal("created", item);
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Rent_WithOnRentCallback_InvokesCallback()
    {
        // Arrange
        var rentedItems = new List<string>();
        using var pool = new ObjectPool<string>(() => "test", rentedItems.Add, null, 0, 5);

        // Act
        var item = pool.Rent();

        // Assert
        Assert.Single(rentedItems);
        Assert.Equal("test", rentedItems[0]);
    }

    [Fact]
    public void Return_ItemToPool_AddsToPool()
    {
        // Arrange
        using var pool = new ObjectPool<string>(() => "test", null, null, 0, 5);
        var item = pool.Rent();

        // Act
        pool.Return(item);

        // Assert
        Assert.Equal(1, pool.Count);
    }

    [Fact]
    public void Return_WithOnReturnCallback_InvokesCallback()
    {
        // Arrange
        var returnedItems = new List<string>();
        using var pool = new ObjectPool<string>(() => "test", null, returnedItems.Add, 0, 5);
        var item = pool.Rent();

        // Act
        pool.Return(item);

        // Assert
        Assert.Single(returnedItems);
        Assert.Equal("test", returnedItems[0]);
    }

    [Fact]
    public void Return_PoolAtMaxSize_DoesNotAddToPool()
    {
        // Arrange
        using var pool = new ObjectPool<string>(() => "test", null, null, 0, 2);
        pool.Return("item1");
        pool.Return("item2");

        // Act
        pool.Return("item3");

        // Assert
        Assert.Equal(2, pool.Count);
    }

    [Fact]
    public void RentAndReturn_ReusesPooledItems()
    {
        // Arrange
        using var pool = new ObjectPool<string>(() => "new", null, null, 0, 5);
        var originalItem = "pooled";
        pool.Return(originalItem);

        // Act
        var rentedItem = pool.Rent();

        // Assert
        Assert.Equal("pooled", rentedItem);
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        // Arrange
        using var pool = new ObjectPool<string>(() => "test", null, null, 0, 5);
        pool.Return("item1");
        pool.Return("item2");
        pool.Return("item3");

        // Act
        pool.Clear();

        // Assert
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Count_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ObjectPool<string>(() => "test", null, null, 0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Count);
    }

    [Fact]
    public void MaxSize_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ObjectPool<string>(() => "test", null, null, 0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.MaxSize);
    }

    [Fact]
    public void Rent_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ObjectPool<string>(() => "test", null, null, 0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(pool.Rent);
    }

    [Fact]
    public void Return_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ObjectPool<string>(() => "test", null, null, 0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Return("test"));
    }

    [Fact]
    public void Clear_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ObjectPool<string>(() => "test", null, null, 0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(pool.Clear);
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        // Arrange
        var pool = new ObjectPool<string>(() => "test", null, null, 0, 5);

        // Act & Assert
        pool.Dispose();
        pool.Dispose(); // Should not throw
    }

    [Fact]
    public void Dispose_WithDisposableItems_DisposesItems()
    {
        // Arrange
        var disposableItems = new List<TestDisposable>();
        using var pool = new ObjectPool<TestDisposable>(() => new TestDisposable(), null, null, 0, 5);

        for (int i = 0; i < 3; i++)
        {
            var item = new TestDisposable();
            disposableItems.Add(item);
            pool.Return(item);
        }

        // Act
        pool.Dispose();

        // Assert
        Assert.All(disposableItems, item => Assert.True(item.IsDisposed));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 10)]
    [InlineData(0, 100)]
    public void Constructor_ValidSizeCombinations_CreatesPool(int initialSize, int maxSize)
    {
        // Arrange & Act
        using var pool = new ObjectPool<string>(() => "test", null, null, initialSize, maxSize);

        // Assert
        Assert.Equal(0, pool.Count);
        Assert.Equal(maxSize, pool.MaxSize);
    }

    [Fact]
    public void RentMultiple_EmptyPool_CreatesMultipleItems()
    {
        // Arrange
        var createdCount = 0;
        using var pool = new ObjectPool<string>(() => $"item{++createdCount}", null, null, 0, 5);

        // Act
        var item1 = pool.Rent();
        var item2 = pool.Rent();
        var item3 = pool.Rent();

        // Assert
        Assert.Equal("item1", item1);
        Assert.Equal("item2", item2);
        Assert.Equal("item3", item3);
        Assert.Equal(0, pool.Count);
    }
}
