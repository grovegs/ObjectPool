using System;
using System.Collections.Generic;

namespace GroveGames.ObjectPool.Tests;

public sealed class ListPoolTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesPool()
    {
        // Arrange & Act
        using var pool = new ListPool<int>(5, 10);

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
        Assert.Throws<ArgumentOutOfRangeException>(() => new ListPool<int>(initialSize, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_NonPositiveMaxSize_ThrowsArgumentOutOfRangeException(int maxSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ListPool<int>(5, maxSize));
    }

    [Fact]
    public void Constructor_InitialSizeGreaterThanMaxSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ListPool<int>(15, 10));
    }

    [Fact]
    public void Rent_EmptyPool_ReturnsNewList()
    {
        // Arrange
        using var pool = new ListPool<string>(0, 5);

        // Act
        var list = pool.Rent();

        // Assert
        Assert.NotNull(list);
        Assert.Empty(list);
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Return_List_AddsToPool()
    {
        // Arrange
        using var pool = new ListPool<string>(0, 5);
        var list = pool.Rent();

        // Act
        pool.Return(list);

        // Assert
        Assert.Equal(1, pool.Count);
    }

    [Fact]
    public void Return_ListWithData_ClearsList()
    {
        // Arrange
        using var pool = new ListPool<int>(0, 5);
        var list = pool.Rent();
        list.Add(1);
        list.Add(2);
        list.Add(3);

        // Act
        pool.Return(list);
        var reusedList = pool.Rent();

        // Assert
        Assert.Same(list, reusedList);
        Assert.Empty(reusedList);
    }

    [Fact]
    public void RentAndReturn_ReusesLists()
    {
        // Arrange
        using var pool = new ListPool<string>(0, 5);
        var originalList = pool.Rent();
        pool.Return(originalList);

        // Act
        var reusedList = pool.Rent();

        // Assert
        Assert.Same(originalList, reusedList);
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Clear_RemovesAllLists()
    {
        // Arrange
        using var pool = new ListPool<int>(0, 5);
        var list1 = pool.Rent();
        var list2 = pool.Rent();
        var list3 = pool.Rent();
        pool.Return(list1);
        pool.Return(list2);
        pool.Return(list3);

        // Act
        pool.Clear();

        // Assert
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Count_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ListPool<int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Count);
    }

    [Fact]
    public void MaxSize_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ListPool<int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.MaxSize);
    }

    [Fact]
    public void Rent_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ListPool<int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(pool.Rent);
    }

    [Fact]
    public void Return_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ListPool<int>(0, 5);
        var list = new List<int>();
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Return(list));
    }

    [Fact]
    public void Clear_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ListPool<int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(pool.Clear);
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        // Arrange
        var pool = new ListPool<int>(0, 5);

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
        using var pool = new ListPool<int>(initialSize, maxSize);

        // Assert
        Assert.Equal(0, pool.Count);
        Assert.Equal(maxSize, pool.MaxSize);
    }

    [Fact]
    public void RentMultiple_EmptyPool_CreatesMultipleLists()
    {
        // Arrange
        using var pool = new ListPool<string>(0, 5);

        // Act
        var list1 = pool.Rent();
        var list2 = pool.Rent();
        var list3 = pool.Rent();

        // Assert
        Assert.NotSame(list1, list2);
        Assert.NotSame(list2, list3);
        Assert.NotSame(list1, list3);
        Assert.Empty(list1);
        Assert.Empty(list2);
        Assert.Empty(list3);
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Return_ListWithMixedOperations_ClearsAllItems()
    {
        // Arrange
        using var pool = new ListPool<int>(0, 5);
        var list = pool.Rent();
        list.Add(10);
        list.Add(20);
        list.Insert(1, 15);
        list.AddRange([30, 40, 50]);

        // Act
        pool.Return(list);
        var reusedList = pool.Rent();

        // Assert
        Assert.Same(list, reusedList);
        Assert.Empty(reusedList);
    }

    [Fact]
    public void Return_MaxSizeReached_DoesNotAddToPool()
    {
        // Arrange
        using var pool = new ListPool<int>(0, 2);
        var list1 = new List<int>();
        var list2 = new List<int>();
        var list3 = new List<int>();

        // Act
        pool.Return(list1);
        pool.Return(list2);
        pool.Return(list3);

        // Assert
        Assert.Equal(2, pool.Count);
    }

    [Fact]
    public void Clear_EmptyPool_DoesNotThrow()
    {
        // Arrange
        using var pool = new ListPool<int>(0, 5);

        // Act & Assert
        pool.Clear(); // Should not throw
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Return_ListWithReferenceTypes_ClearsCorrectly()
    {
        // Arrange
        using var pool = new ListPool<string>(0, 5);
        var list = pool.Rent();
        list.Add("first");
        list.Add("second");
        list.Add("third");

        // Act
        pool.Return(list);
        var reusedList = pool.Rent();

        // Assert
        Assert.Same(list, reusedList);
        Assert.Empty(reusedList);
    }

    [Fact]
    public void List_AfterClear_BehavesLikeNewList()
    {
        // Arrange
        using var pool = new ListPool<int>(0, 5);
        var list = pool.Rent();
        list.AddRange([1, 2, 3, 4, 5]);
        pool.Return(list);

        // Act
        var reusedList = pool.Rent();
        reusedList.Add(100);
        reusedList.Add(200);

        // Assert
        Assert.Same(list, reusedList);
        Assert.Equal(2, reusedList.Count);
        Assert.Equal(100, reusedList[0]);
        Assert.Equal(200, reusedList[1]);
    }

    [Fact]
    public void Pool_WithLargeDataSet_HandlesClearingCorrectly()
    {
        // Arrange
        using var pool = new ListPool<int>(0, 5);
        var list = pool.Rent();

        for (int i = 0; i < 10000; i++)
        {
            list.Add(i);
        }

        // Act
        pool.Return(list);
        var reusedList = pool.Rent();

        // Assert
        Assert.Same(list, reusedList);
        Assert.Empty(reusedList);
    }

    [Fact]
    public void Return_ListWithComplexObjects_ClearsCorrectly()
    {
        // Arrange
        using var pool = new ListPool<List<int>>(0, 5);
        var list = pool.Rent();
        list.Add([1, 2, 3]);
        list.Add([4, 5, 6]);
        list.Add([7, 8, 9]);

        // Act
        pool.Return(list);
        var reusedList = pool.Rent();

        // Assert
        Assert.Same(list, reusedList);
        Assert.Empty(reusedList);
    }

    [Fact]
    public void List_CapacityPreserved_AfterClear()
    {
        // Arrange
        using var pool = new ListPool<int>(0, 5);
        var list = pool.Rent();

        for (int i = 0; i < 1000; i++)
        {
            list.Add(i);
        }
        var originalCapacity = list.Capacity;
        pool.Return(list);

        // Act
        var reusedList = pool.Rent();

        // Assert
        Assert.Same(list, reusedList);
        Assert.Empty(reusedList);
        Assert.Equal(originalCapacity, reusedList.Capacity);
    }

    [Fact]
    public void Return_ListAfterIndexOperations_ClearsCorrectly()
    {
        // Arrange
        using var pool = new ListPool<string>(0, 5);
        var list = pool.Rent();
        list.Add("a");
        list.Add("b");
        list.Add("c");
        list[1] = "modified";
        list.RemoveAt(0);

        // Act
        pool.Return(list);
        var reusedList = pool.Rent();

        // Assert
        Assert.Same(list, reusedList);
        Assert.Empty(reusedList);
    }

    [Fact]
    public void Pool_WithValueTypes_HandlesCorrectly()
    {
        // Arrange
        using var pool = new ListPool<DateTime>(0, 5);
        var list = pool.Rent();
        list.Add(DateTime.Now);
        list.Add(DateTime.UtcNow);
        list.Add(DateTime.Today);

        // Act
        pool.Return(list);
        var reusedList = pool.Rent();

        // Assert
        Assert.Same(list, reusedList);
        Assert.Empty(reusedList);
    }
}
