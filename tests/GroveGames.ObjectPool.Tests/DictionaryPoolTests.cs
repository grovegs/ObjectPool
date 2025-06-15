namespace GroveGames.ObjectPool.Tests;

public sealed class DictionaryPoolTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesPool()
    {
        // Arrange & Act
        using var pool = new DictionaryPool<string, int>(5, 10);

        // Assert
        Assert.Equal(0, pool.Count);
        Assert.Equal(10, pool.MaxSize);
    }

    [Fact]
    public void Constructor_WithComparer_CreatesPool()
    {
        // Arrange & Act
        using var pool = new DictionaryPool<string, int>(5, 10, StringComparer.OrdinalIgnoreCase);

        // Assert
        Assert.Equal(0, pool.Count);
        Assert.Equal(10, pool.MaxSize);
    }

    [Fact]
    public void Constructor_WithNullComparer_CreatesPool()
    {
        // Arrange & Act
        using var pool = new DictionaryPool<string, int>(5, 10, null);

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
        Assert.Throws<ArgumentOutOfRangeException>(() => new DictionaryPool<string, int>(initialSize, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_NonPositiveMaxSize_ThrowsArgumentOutOfRangeException(int maxSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new DictionaryPool<string, int>(5, maxSize));
    }

    [Fact]
    public void Constructor_InitialSizeGreaterThanMaxSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new DictionaryPool<string, int>(15, 10));
    }

    [Fact]
    public void Rent_EmptyPool_ReturnsNewDictionary()
    {
        // Arrange
        using var pool = new DictionaryPool<string, int>(0, 5);

        // Act
        var dictionary = pool.Rent();

        // Assert
        Assert.NotNull(dictionary);
        Assert.Empty(dictionary);
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Rent_WithComparer_ReturnsNewDictionaryWithComparer()
    {
        // Arrange
        using var pool = new DictionaryPool<string, int>(0, 5, StringComparer.OrdinalIgnoreCase);

        // Act
        var dictionary = pool.Rent();
        dictionary["Key"] = 1;
        dictionary["key"] = 2;

        // Assert
        Assert.Single(dictionary);
        Assert.Equal(2, dictionary["KEY"]);
    }

    [Fact]
    public void Return_Dictionary_AddsToPool()
    {
        // Arrange
        using var pool = new DictionaryPool<string, int>(0, 5);
        var dictionary = pool.Rent();

        // Act
        pool.Return(dictionary);

        // Assert
        Assert.Equal(1, pool.Count);
    }

    [Fact]
    public void Return_DictionaryWithData_ClearsDictionary()
    {
        // Arrange
        using var pool = new DictionaryPool<string, int>(0, 5);
        var dictionary = pool.Rent();
        dictionary["key1"] = 100;
        dictionary["key2"] = 200;

        // Act
        pool.Return(dictionary);
        var reusedDictionary = pool.Rent();

        // Assert
        Assert.Same(dictionary, reusedDictionary);
        Assert.Empty(reusedDictionary);
    }

    [Fact]
    public void RentAndReturn_ReusesDictionaries()
    {
        // Arrange
        using var pool = new DictionaryPool<string, int>(0, 5);
        var originalDictionary = pool.Rent();
        pool.Return(originalDictionary);

        // Act
        var reusedDictionary = pool.Rent();

        // Assert
        Assert.Same(originalDictionary, reusedDictionary);
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Clear_RemovesAllDictionaries()
    {
        // Arrange
        using var pool = new DictionaryPool<string, int>(0, 5);
        var dict1 = pool.Rent();
        var dict2 = pool.Rent();
        var dict3 = pool.Rent();
        pool.Return(dict1);
        pool.Return(dict2);
        pool.Return(dict3);

        // Act
        pool.Clear();

        // Assert
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Count_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new DictionaryPool<string, int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Count);
    }

    [Fact]
    public void MaxSize_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new DictionaryPool<string, int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.MaxSize);
    }

    [Fact]
    public void Rent_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new DictionaryPool<string, int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(pool.Rent);
    }

    [Fact]
    public void Return_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new DictionaryPool<string, int>(0, 5);
        var dictionary = new Dictionary<string, int>();
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Return(dictionary));
    }

    [Fact]
    public void Clear_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new DictionaryPool<string, int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(pool.Clear);
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        // Arrange
        var pool = new DictionaryPool<string, int>(0, 5);

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
        using var pool = new DictionaryPool<string, int>(initialSize, maxSize);

        // Assert
        Assert.Equal(0, pool.Count);
        Assert.Equal(maxSize, pool.MaxSize);
    }

    [Fact]
    public void RentMultiple_EmptyPool_CreatesMultipleDictionaries()
    {
        // Arrange
        using var pool = new DictionaryPool<string, int>(0, 5);

        // Act
        var dict1 = pool.Rent();
        var dict2 = pool.Rent();
        var dict3 = pool.Rent();

        // Assert
        Assert.NotSame(dict1, dict2);
        Assert.NotSame(dict2, dict3);
        Assert.NotSame(dict1, dict3);
        Assert.Empty(dict1);
        Assert.Empty(dict2);
        Assert.Empty(dict3);
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Return_DictionaryWithMixedData_ClearsAllEntries()
    {
        // Arrange
        using var pool = new DictionaryPool<string, object>(0, 5);
        var dictionary = pool.Rent();
        dictionary["string"] = "value";
        dictionary["number"] = 42;
        dictionary["null"] = null!;

        // Act
        pool.Return(dictionary);
        var reusedDictionary = pool.Rent();

        // Assert
        Assert.Same(dictionary, reusedDictionary);
        Assert.Empty(reusedDictionary);
        Assert.False(reusedDictionary.ContainsKey("string"));
        Assert.False(reusedDictionary.ContainsKey("number"));
        Assert.False(reusedDictionary.ContainsKey("null"));
    }

    [Fact]
    public void Rent_WithCustomComparer_PreservesComparerBehavior()
    {
        // Arrange
        using var pool = new DictionaryPool<string, int>(0, 5, StringComparer.OrdinalIgnoreCase);

        // Act
        var dict1 = pool.Rent();
        dict1["Test"] = 1;
        pool.Return(dict1);

        var dict2 = pool.Rent();
        dict2["test"] = 2;
        dict2["TEST"] = 3;

        // Assert
        Assert.Same(dict1, dict2);
        Assert.Single(dict2);
        Assert.Equal(3, dict2["test"]);
    }

    [Fact]
    public void Return_MaxSizeReached_DoesNotAddToPool()
    {
        // Arrange
        using var pool = new DictionaryPool<string, int>(0, 2);
        var dict1 = new Dictionary<string, int>();
        var dict2 = new Dictionary<string, int>();
        var dict3 = new Dictionary<string, int>();

        // Act
        pool.Return(dict1);
        pool.Return(dict2);
        pool.Return(dict3); // Should not be added due to max size

        // Assert
        Assert.Equal(2, pool.Count);
    }

    [Fact]
    public void Clear_EmptyPool_DoesNotThrow()
    {
        // Arrange
        using var pool = new DictionaryPool<string, int>(0, 5);

        // Act & Assert
        pool.Clear(); // Should not throw
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Pool_WithComplexTypes_HandlesCorrectly()
    {
        // Arrange
        using var pool = new DictionaryPool<int, List<string>>(0, 5);

        // Act
        var dictionary = pool.Rent();
        dictionary[1] = ["a", "b"];
        dictionary[2] = ["c", "d"];
        pool.Return(dictionary);

        var reusedDictionary = pool.Rent();

        // Assert
        Assert.Same(dictionary, reusedDictionary);
        Assert.Empty(reusedDictionary);
    }
}
