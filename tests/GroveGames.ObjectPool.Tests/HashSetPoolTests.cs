namespace GroveGames.ObjectPool.Tests;

public sealed class HashSetPoolTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesPool()
    {
        // Arrange & Act
        using var pool = new HashSetPool<int>(5, 10);

        // Assert
        Assert.Equal(0, pool.Count);
        Assert.Equal(10, pool.MaxSize);
    }

    [Fact]
    public void Constructor_WithComparer_CreatesPool()
    {
        // Arrange & Act
        using var pool = new HashSetPool<string>(5, 10, StringComparer.OrdinalIgnoreCase);

        // Assert
        Assert.Equal(0, pool.Count);
        Assert.Equal(10, pool.MaxSize);
    }

    [Fact]
    public void Constructor_WithNullComparer_CreatesPool()
    {
        // Arrange & Act
        using var pool = new HashSetPool<string>(5, 10, null);

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
        Assert.Throws<ArgumentOutOfRangeException>(() => new HashSetPool<int>(initialSize, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_NonPositiveMaxSize_ThrowsArgumentOutOfRangeException(int maxSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new HashSetPool<int>(5, maxSize));
    }

    [Fact]
    public void Constructor_InitialSizeGreaterThanMaxSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new HashSetPool<int>(15, 10));
    }

    [Fact]
    public void Rent_EmptyPool_ReturnsNewHashSet()
    {
        // Arrange
        using var pool = new HashSetPool<string>(0, 5);

        // Act
        var hashSet = pool.Rent();

        // Assert
        Assert.NotNull(hashSet);
        Assert.Empty(hashSet);
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Rent_WithComparer_ReturnsHashSetWithComparer()
    {
        // Arrange
        using var pool = new HashSetPool<string>(0, 5, StringComparer.OrdinalIgnoreCase);

        // Act
        var hashSet = pool.Rent();
        hashSet.Add("Test");
        hashSet.Add("test");

        // Assert
        Assert.Single(hashSet);
        Assert.Contains("TEST", hashSet);
    }

    [Fact]
    public void Return_HashSet_AddsToPool()
    {
        // Arrange
        using var pool = new HashSetPool<string>(0, 5);
        var hashSet = pool.Rent();

        // Act
        pool.Return(hashSet);

        // Assert
        Assert.Equal(1, pool.Count);
    }

    [Fact]
    public void Return_HashSetWithData_ClearsHashSet()
    {
        // Arrange
        using var pool = new HashSetPool<int>(0, 5);
        var hashSet = pool.Rent();
        hashSet.Add(1);
        hashSet.Add(2);
        hashSet.Add(3);

        // Act
        pool.Return(hashSet);
        var reusedHashSet = pool.Rent();

        // Assert
        Assert.Same(hashSet, reusedHashSet);
        Assert.Empty(reusedHashSet);
    }

    [Fact]
    public void RentAndReturn_ReusesHashSets()
    {
        // Arrange
        using var pool = new HashSetPool<string>(0, 5);
        var originalHashSet = pool.Rent();
        pool.Return(originalHashSet);

        // Act
        var reusedHashSet = pool.Rent();

        // Assert
        Assert.Same(originalHashSet, reusedHashSet);
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Clear_RemovesAllHashSets()
    {
        // Arrange
        using var pool = new HashSetPool<int>(0, 5);
        var set1 = pool.Rent();
        var set2 = pool.Rent();
        var set3 = pool.Rent();
        pool.Return(set1);
        pool.Return(set2);
        pool.Return(set3);

        // Act
        pool.Clear();

        // Assert
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Count_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new HashSetPool<int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Count);
    }

    [Fact]
    public void MaxSize_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new HashSetPool<int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.MaxSize);
    }

    [Fact]
    public void Rent_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new HashSetPool<int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Rent());
    }

    [Fact]
    public void Return_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new HashSetPool<int>(0, 5);
        var hashSet = new HashSet<int>();
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Return(hashSet));
    }

    [Fact]
    public void Clear_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new HashSetPool<int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Clear());
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        // Arrange
        var pool = new HashSetPool<int>(0, 5);

        // Act & Assert
        pool.Dispose();
        pool.Dispose();
    }

    [Fact]
    public async Task RentAndReturn_ConcurrentOperations_ThreadSafe()
    {
        // Arrange
        using var pool = new HashSetPool<int>(0, 100);
        var rentedHashSets = new List<HashSet<int>>();

        // Act
        var tasks = Enumerable.Range(0, 50).Select(async i =>
        {
            await Task.Yield();
            var hashSet = pool.Rent();
            hashSet.Add(i);
            lock (rentedHashSets)
            {
                rentedHashSets.Add(hashSet);
            }
            pool.Return(hashSet);
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(50, rentedHashSets.Count);
        Assert.True(pool.Count <= 100);
    }

    [Fact]
    public async Task Return_ConcurrentReturns_RespectMaxSize()
    {
        // Arrange
        using var pool = new HashSetPool<int>(0, 10);
        var hashSets = Enumerable.Range(0, 20).Select(_ => new HashSet<int>()).ToArray();

        // Act
        var tasks = hashSets.Select(async hashSet =>
        {
            await Task.Yield();
            pool.Return(hashSet);
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.True(pool.Count <= 10);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 10)]
    [InlineData(0, 100)]
    public void Constructor_ValidSizeCombinations_CreatesPool(int initialSize, int maxSize)
    {
        // Arrange & Act
        using var pool = new HashSetPool<int>(initialSize, maxSize);

        // Assert
        Assert.Equal(0, pool.Count);
        Assert.Equal(maxSize, pool.MaxSize);
    }

    [Fact]
    public void RentMultiple_EmptyPool_CreatesMultipleHashSets()
    {
        // Arrange
        using var pool = new HashSetPool<string>(0, 5);

        // Act
        var set1 = pool.Rent();
        var set2 = pool.Rent();
        var set3 = pool.Rent();

        // Assert
        Assert.NotSame(set1, set2);
        Assert.NotSame(set2, set3);
        Assert.NotSame(set1, set3);
        Assert.Empty(set1);
        Assert.Empty(set2);
        Assert.Empty(set3);
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Return_HashSetWithSetOperations_ClearsAllItems()
    {
        // Arrange
        using var pool = new HashSetPool<int>(0, 5);
        var hashSet = pool.Rent();
        hashSet.Add(1);
        hashSet.Add(2);
        hashSet.Add(3);
        hashSet.Remove(2);
        hashSet.UnionWith(new[] { 4, 5, 6 });

        // Act
        pool.Return(hashSet);
        var reusedHashSet = pool.Rent();

        // Assert
        Assert.Same(hashSet, reusedHashSet);
        Assert.Empty(reusedHashSet);
    }

    [Fact]
    public void Return_MaxSizeReached_DoesNotAddToPool()
    {
        // Arrange
        using var pool = new HashSetPool<int>(0, 2);
        var set1 = new HashSet<int>();
        var set2 = new HashSet<int>();
        var set3 = new HashSet<int>();

        // Act
        pool.Return(set1);
        pool.Return(set2);
        pool.Return(set3);

        // Assert
        Assert.Equal(2, pool.Count);
    }

    [Fact]
    public void Clear_EmptyPool_DoesNotThrow()
    {
        // Arrange
        using var pool = new HashSetPool<int>(0, 5);

        // Act & Assert
        pool.Clear();
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Return_HashSetWithReferenceTypes_ClearsCorrectly()
    {
        // Arrange
        using var pool = new HashSetPool<string>(0, 5);
        var hashSet = pool.Rent();
        hashSet.Add("first");
        hashSet.Add("second");
        hashSet.Add("third");

        // Act
        pool.Return(hashSet);
        var reusedHashSet = pool.Rent();

        // Assert
        Assert.Same(hashSet, reusedHashSet);
        Assert.Empty(reusedHashSet);
    }

    [Fact]
    public void HashSet_AfterClear_BehavesLikeNewHashSet()
    {
        // Arrange
        using var pool = new HashSetPool<int>(0, 5);
        var hashSet = pool.Rent();
        hashSet.UnionWith(new[] { 1, 2, 3, 4, 5 });
        pool.Return(hashSet);

        // Act
        var reusedHashSet = pool.Rent();
        reusedHashSet.Add(100);
        reusedHashSet.Add(200);

        // Assert
        Assert.Same(hashSet, reusedHashSet);
        Assert.Equal(2, reusedHashSet.Count);
        Assert.Contains(100, reusedHashSet);
        Assert.Contains(200, reusedHashSet);
    }

    [Fact]
    public void Pool_WithLargeDataSet_HandlesClearingCorrectly()
    {
        // Arrange
        using var pool = new HashSetPool<int>(0, 5);
        var hashSet = pool.Rent();

        for (int i = 0; i < 1000; i++)
        {
            hashSet.Add(i);
        }

        // Act
        pool.Return(hashSet);
        var reusedHashSet = pool.Rent();

        // Assert
        Assert.Same(hashSet, reusedHashSet);
        Assert.Empty(reusedHashSet);
    }

    [Fact]
    public void Return_HashSetWithComplexObjects_ClearsCorrectly()
    {
        // Arrange
        using var pool = new HashSetPool<List<int>>(0, 5);
        var hashSet = pool.Rent();
        hashSet.Add(new List<int> { 1, 2, 3 });
        hashSet.Add(new List<int> { 4, 5, 6 });
        hashSet.Add(new List<int> { 7, 8, 9 });

        // Act
        pool.Return(hashSet);
        var reusedHashSet = pool.Rent();

        // Assert
        Assert.Same(hashSet, reusedHashSet);
        Assert.Empty(reusedHashSet);
    }

    [Fact]
    public void HashSet_SetOperations_WorkCorrectlyAfterClear()
    {
        // Arrange
        using var pool = new HashSetPool<int>(0, 5);
        var hashSet = pool.Rent();
        hashSet.UnionWith(new[] { 1, 2, 3 });
        pool.Return(hashSet);

        // Act
        var reusedHashSet = pool.Rent();
        reusedHashSet.UnionWith(new[] { 10, 20 });
        reusedHashSet.IntersectWith(new[] { 10, 30 });

        // Assert
        Assert.Same(hashSet, reusedHashSet);
        Assert.Single(reusedHashSet);
        Assert.Contains(10, reusedHashSet);
    }

    [Fact]
    public void Return_HashSetWithDuplicateAttempts_ClearsCorrectly()
    {
        // Arrange
        using var pool = new HashSetPool<string>(0, 5);
        var hashSet = pool.Rent();
        hashSet.Add("duplicate");
        hashSet.Add("duplicate"); // Should not add twice
        hashSet.Add("unique");

        // Act
        pool.Return(hashSet);
        var reusedHashSet = pool.Rent();

        // Assert
        Assert.Same(hashSet, reusedHashSet);
        Assert.Empty(reusedHashSet);
    }

    [Fact]
    public void Pool_WithValueTypes_HandlesCorrectly()
    {
        // Arrange
        using var pool = new HashSetPool<DateTime>(0, 5);
        var hashSet = pool.Rent();
        hashSet.Add(DateTime.Now);
        hashSet.Add(DateTime.UtcNow);
        hashSet.Add(DateTime.Today);

        // Act
        pool.Return(hashSet);
        var reusedHashSet = pool.Rent();

        // Assert
        Assert.Same(hashSet, reusedHashSet);
        Assert.Empty(reusedHashSet);
    }

    [Fact]
    public void Rent_WithComparer_PreservesComparerBehavior()
    {
        // Arrange
        using var pool = new HashSetPool<string>(0, 5, StringComparer.OrdinalIgnoreCase);

        // Act
        var set1 = pool.Rent();
        set1.Add("Test");
        pool.Return(set1);

        var set2 = pool.Rent();
        set2.Add("test");
        set2.Add("TEST");

        // Assert
        Assert.Same(set1, set2);
        Assert.Single(set2);
        Assert.Contains("test", set2);
    }

    [Fact]
    public void HashSet_ExceptWithOperation_WorksCorrectlyAfterClear()
    {
        // Arrange
        using var pool = new HashSetPool<int>(0, 5);
        var hashSet = pool.Rent();
        hashSet.UnionWith(new[] { 1, 2, 3, 4, 5 });
        pool.Return(hashSet);

        // Act
        var reusedHashSet = pool.Rent();
        reusedHashSet.UnionWith(new[] { 10, 20, 30 });
        reusedHashSet.ExceptWith(new[] { 20 });

        // Assert
        Assert.Same(hashSet, reusedHashSet);
        Assert.Equal(2, reusedHashSet.Count);
        Assert.Contains(10, reusedHashSet);
        Assert.Contains(30, reusedHashSet);
        Assert.DoesNotContain(20, reusedHashSet);
    }

    [Fact]
    public void HashSet_SymmetricExceptWithOperation_WorksCorrectlyAfterClear()
    {
        // Arrange
        using var pool = new HashSetPool<int>(0, 5);
        var hashSet = pool.Rent();
        hashSet.UnionWith(new[] { 1, 2, 3 });
        pool.Return(hashSet);

        // Act
        var reusedHashSet = pool.Rent();
        reusedHashSet.UnionWith(new[] { 10, 20 });
        reusedHashSet.SymmetricExceptWith(new[] { 20, 30 });

        // Assert
        Assert.Same(hashSet, reusedHashSet);
        Assert.Equal(2, reusedHashSet.Count);
        Assert.Contains(10, reusedHashSet);
        Assert.Contains(30, reusedHashSet);
        Assert.DoesNotContain(20, reusedHashSet);
    }

    [Fact]
    public void HashSet_IsSubsetOfOperation_WorksCorrectlyAfterClear()
    {
        // Arrange
        using var pool = new HashSetPool<int>(0, 5);
        var hashSet = pool.Rent();
        hashSet.UnionWith(new[] { 1, 2, 3, 4, 5 });
        pool.Return(hashSet);

        // Act
        var reusedHashSet = pool.Rent();
        reusedHashSet.UnionWith(new[] { 10, 20 });
        var isSubset = reusedHashSet.IsSubsetOf(new[] { 10, 20, 30 });

        // Assert
        Assert.Same(hashSet, reusedHashSet);
        Assert.True(isSubset);
    }
}