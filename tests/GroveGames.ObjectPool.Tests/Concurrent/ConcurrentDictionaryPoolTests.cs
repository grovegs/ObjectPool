using GroveGames.ObjectPool.Concurrent;

namespace GroveGames.ObjectPool.Tests.Concurrent;

public sealed class ConcurrentDictionaryPoolTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesPool()
    {
        // Act
        using var pool = new ConcurrentDictionaryPool<string, int>(10, 20);

        // Assert
        Assert.Equal(20, pool.MaxSize);
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Constructor_ValidParametersWithComparer_CreatesPool()
    {
        // Arrange
        var comparer = StringComparer.OrdinalIgnoreCase;

        // Act
        using var pool = new ConcurrentDictionaryPool<string, int>(5, 10, comparer);

        // Assert
        Assert.Equal(10, pool.MaxSize);
        Assert.Equal(0, pool.Count);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Constructor_NegativeInitialSize_ThrowsArgumentOutOfRangeException(int initialSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ConcurrentDictionaryPool<string, int>(initialSize, 10));
    }

    [Theory]
    [InlineData(11, 10)]
    [InlineData(20, 15)]
    public void Constructor_InitialSizeGreaterThanMaxSize_ThrowsArgumentOutOfRangeException(int initialSize, int maxSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ConcurrentDictionaryPool<string, int>(initialSize, maxSize));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Constructor_NonPositiveMaxSize_ThrowsArgumentOutOfRangeException(int maxSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ConcurrentDictionaryPool<string, int>(0, maxSize));
    }

    [Fact]
    public void Rent_FromNewPool_ReturnsDictionary()
    {
        // Arrange
        using var pool = new ConcurrentDictionaryPool<string, int>(5, 10);

        // Act
        var dictionary = pool.Rent();

        // Assert
        Assert.NotNull(dictionary);
        Assert.Empty(dictionary);
    }

    [Fact]
    public void Rent_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentDictionaryPool<string, int>(5, 10);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Rent());
    }

    [Fact]
    public void Return_ValidDictionary_ReturnsToPoo()
    {
        // Arrange
        using var pool = new ConcurrentDictionaryPool<string, int>(0, 10);
        var dictionary = pool.Rent();
        dictionary["key"] = 42;

        // Act
        pool.Return(dictionary);
        var rentedAgain = pool.Rent();

        // Assert
        Assert.Same(dictionary, rentedAgain);
        Assert.Empty(rentedAgain);
    }

    [Fact]
    public void Return_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        using var pool = new ConcurrentDictionaryPool<string, int>(5, 10);
        var dictionary = pool.Rent();
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Return(dictionary));
    }

    [Fact]
    public void Clear_RemovesAllPooledDictionaries()
    {
        // Arrange
        using var pool = new ConcurrentDictionaryPool<string, int>(5, 10);
        var dict1 = pool.Rent();
        var dict2 = pool.Rent();
        pool.Return(dict1);
        pool.Return(dict2);
        Assert.Equal(2, pool.Count);

        // Act
        pool.Clear();

        // Assert
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Clear_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentDictionaryPool<string, int>(5, 10);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Clear());
    }

    [Fact]
    public void Count_BeforeDispose_ReturnsCurrentCount()
    {
        // Arrange
        using var pool = new ConcurrentDictionaryPool<string, int>(5, 10);
        var dict = pool.Rent();
        pool.Return(dict);

        // Act
        var count = pool.Count;

        // Assert
        Assert.Equal(1, count);
    }

    [Fact]
    public void Count_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentDictionaryPool<string, int>(5, 10);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Count);
    }

    [Fact]
    public void MaxSize_BeforeDispose_ReturnsMaxSize()
    {
        // Arrange
        using var pool = new ConcurrentDictionaryPool<string, int>(5, 10);

        // Act
        var maxSize = pool.MaxSize;

        // Assert
        Assert.Equal(10, maxSize);
    }

    [Fact]
    public void MaxSize_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentDictionaryPool<string, int>(5, 10);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.MaxSize);
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var pool = new ConcurrentDictionaryPool<string, int>(5, 10);

        // Act & Assert
        pool.Dispose();
        pool.Dispose();
        pool.Dispose();
    }

    [Fact]
    public void Rent_WithCustomComparer_UsesSameComparer()
    {
        // Arrange
        var comparer = StringComparer.OrdinalIgnoreCase;
        using var pool = new ConcurrentDictionaryPool<string, int>(1, 10, comparer);

        // Act
        var dictionary = pool.Rent();
        dictionary["Key"] = 1;
        var hasKey = dictionary.ContainsKey("key");

        // Assert
        Assert.True(hasKey);
    }

    [Fact]
    public async Task Rent_ConcurrentRents_AllSucceed()
    {
        // Arrange
        using var pool = new ConcurrentDictionaryPool<string, int>(0, 100);
        var dictionaries = new Dictionary<string, int>[50];

        // Act
        var tasks = Enumerable.Range(0, 50).Select(i => Task.Run(() =>
        {
            dictionaries[i] = pool.Rent();
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.All(dictionaries, d => Assert.NotNull(d));
        Assert.Equal(50, dictionaries.Distinct().Count());
    }

    [Fact]
    public async Task Return_ConcurrentReturns_AllSucceed()
    {
        // Arrange
        using var pool = new ConcurrentDictionaryPool<string, int>(0, 100);
        var dictionaries = Enumerable.Range(0, 50)
            .Select(_ => pool.Rent())
            .ToArray();

        // Act
        var tasks = dictionaries.Select(d => Task.Run(() =>
        {
            d["test"] = 1;
            pool.Return(d);
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(50, pool.Count);
    }

    [Fact]
    public async Task RentAndReturn_ConcurrentMixed_AllSucceed()
    {
        // Arrange
        using var pool = new ConcurrentDictionaryPool<string, int>(10, 100);
        var rentCount = 0;
        var returnCount = 0;

        // Act
        var tasks = Enumerable.Range(0, 100).Select(i => Task.Run(() =>
        {
            if (i % 2 == 0)
            {
                var dict = pool.Rent();
                Interlocked.Increment(ref rentCount);
                dict[$"key{i}"] = i;
                pool.Return(dict);
                Interlocked.Increment(ref returnCount);
            }
            else
            {
                var dict = pool.Rent();
                Interlocked.Increment(ref rentCount);
                pool.Return(dict);
                Interlocked.Increment(ref returnCount);
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(100, rentCount);
        Assert.Equal(100, returnCount);
    }

    [Fact]
    public void Return_DictionaryIsCleared_BeforeReuse()
    {
        // Arrange
        using var pool = new ConcurrentDictionaryPool<string, int>(0, 10);
        var dictionary = pool.Rent();
        dictionary["key1"] = 1;
        dictionary["key2"] = 2;

        // Act
        pool.Return(dictionary);
        var rentedAgain = pool.Rent();

        // Assert
        Assert.Same(dictionary, rentedAgain);
        Assert.Empty(rentedAgain);
    }
}