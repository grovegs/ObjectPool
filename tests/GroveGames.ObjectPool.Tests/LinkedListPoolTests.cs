namespace GroveGames.ObjectPool.Tests;

public sealed class LinkedListPoolTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesPool()
    {
        // Arrange & Act
        using var pool = new LinkedListPool<int>(5, 10);

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
        Assert.Throws<ArgumentOutOfRangeException>(() => new LinkedListPool<int>(initialSize, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_NonPositiveMaxSize_ThrowsArgumentOutOfRangeException(int maxSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new LinkedListPool<int>(5, maxSize));
    }

    [Fact]
    public void Constructor_InitialSizeGreaterThanMaxSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new LinkedListPool<int>(15, 10));
    }

    [Fact]
    public void Rent_EmptyPool_ReturnsNewLinkedList()
    {
        // Arrange
        using var pool = new LinkedListPool<string>(0, 5);

        // Act
        var linkedList = pool.Rent();

        // Assert
        Assert.NotNull(linkedList);
        Assert.Empty(linkedList);
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Return_LinkedList_AddsToPool()
    {
        // Arrange
        using var pool = new LinkedListPool<string>(0, 5);
        var linkedList = pool.Rent();

        // Act
        pool.Return(linkedList);

        // Assert
        Assert.Equal(1, pool.Count);
    }

    [Fact]
    public void Return_LinkedListWithData_ClearsLinkedList()
    {
        // Arrange
        using var pool = new LinkedListPool<int>(0, 5);
        var linkedList = pool.Rent();
        linkedList.AddLast(1);
        linkedList.AddLast(2);
        linkedList.AddLast(3);

        // Act
        pool.Return(linkedList);
        var reusedLinkedList = pool.Rent();

        // Assert
        Assert.Same(linkedList, reusedLinkedList);
        Assert.Empty(reusedLinkedList);
    }

    [Fact]
    public void RentAndReturn_ReusesLinkedLists()
    {
        // Arrange
        using var pool = new LinkedListPool<string>(0, 5);
        var originalLinkedList = pool.Rent();
        pool.Return(originalLinkedList);

        // Act
        var reusedLinkedList = pool.Rent();

        // Assert
        Assert.Same(originalLinkedList, reusedLinkedList);
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Clear_RemovesAllLinkedLists()
    {
        // Arrange
        using var pool = new LinkedListPool<int>(0, 5);
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
        var pool = new LinkedListPool<int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Count);
    }

    [Fact]
    public void MaxSize_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new LinkedListPool<int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.MaxSize);
    }

    [Fact]
    public void Rent_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new LinkedListPool<int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(pool.Rent);
    }

    [Fact]
    public void Return_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new LinkedListPool<int>(0, 5);
        var linkedList = new LinkedList<int>();
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Return(linkedList));
    }

    [Fact]
    public void Clear_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new LinkedListPool<int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(pool.Clear);
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        // Arrange
        var pool = new LinkedListPool<int>(0, 5);

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
        using var pool = new LinkedListPool<int>(initialSize, maxSize);

        // Assert
        Assert.Equal(0, pool.Count);
        Assert.Equal(maxSize, pool.MaxSize);
    }

    [Fact]
    public void RentMultiple_EmptyPool_CreatesMultipleLinkedLists()
    {
        // Arrange
        using var pool = new LinkedListPool<string>(0, 5);

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
    public void Return_LinkedListWithComplexData_ClearsAllNodes()
    {
        // Arrange
        using var pool = new LinkedListPool<string>(0, 5);
        var linkedList = pool.Rent();
        linkedList.AddFirst("first");
        linkedList.AddLast("last");
        linkedList.AddAfter(linkedList.First!, "middle");

        // Act
        pool.Return(linkedList);
        var reusedLinkedList = pool.Rent();

        // Assert
        Assert.Same(linkedList, reusedLinkedList);
        Assert.Empty(reusedLinkedList);
        Assert.Null(reusedLinkedList.First);
        Assert.Null(reusedLinkedList.Last);
    }

    [Fact]
    public void Return_MaxSizeReached_DoesNotAddToPool()
    {
        // Arrange
        using var pool = new LinkedListPool<int>(0, 2);
        var list1 = new LinkedList<int>();
        var list2 = new LinkedList<int>();
        var list3 = new LinkedList<int>();

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
        using var pool = new LinkedListPool<int>(0, 5);

        // Act & Assert
        pool.Clear();
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Return_LinkedListWithReferenceTypes_ClearsCorrectly()
    {
        // Arrange
        using var pool = new LinkedListPool<List<int>>(0, 5);
        var linkedList = pool.Rent();
        linkedList.AddLast([1, 2, 3]);
        linkedList.AddLast([4, 5, 6]);

        // Act
        pool.Return(linkedList);
        var reusedLinkedList = pool.Rent();

        // Assert
        Assert.Same(linkedList, reusedLinkedList);
        Assert.Empty(reusedLinkedList);
    }

    [Fact]
    public void LinkedList_AfterClear_BehavesLikeNewLinkedList()
    {
        // Arrange
        using var pool = new LinkedListPool<int>(0, 5);
        var linkedList = pool.Rent();
        linkedList.AddFirst(10);
        linkedList.AddLast(20);
        linkedList.AddLast(30);
        pool.Return(linkedList);

        // Act
        var reusedLinkedList = pool.Rent();
        reusedLinkedList.AddFirst(100);
        reusedLinkedList.AddLast(200);

        // Assert
        Assert.Same(linkedList, reusedLinkedList);
        Assert.Equal(2, reusedLinkedList.Count);
        Assert.Equal(100, reusedLinkedList.First!.Value);
        Assert.Equal(200, reusedLinkedList.Last!.Value);
    }

    [Fact]
    public void Pool_WithLargeDataSet_HandlesClearingCorrectly()
    {
        // Arrange
        using var pool = new LinkedListPool<int>(0, 5);
        var linkedList = pool.Rent();

        for (int i = 0; i < 1000; i++)
        {
            linkedList.AddLast(i);
        }

        // Act
        pool.Return(linkedList);
        var reusedLinkedList = pool.Rent();

        // Assert
        Assert.Same(linkedList, reusedLinkedList);
        Assert.Empty(reusedLinkedList);
    }

    [Fact]
    public void LinkedList_NodeReferences_ClearedAfterReturn()
    {
        // Arrange
        using var pool = new LinkedListPool<string>(0, 5);
        var linkedList = pool.Rent();
        var firstNode = linkedList.AddFirst("first");
        var lastNode = linkedList.AddLast("last");
        pool.Return(linkedList);

        // Act
        var reusedLinkedList = pool.Rent();

        // Assert
        Assert.Same(linkedList, reusedLinkedList);
        Assert.Empty(reusedLinkedList);
        Assert.Null(firstNode.List);
        Assert.Null(lastNode.List);
    }
}
