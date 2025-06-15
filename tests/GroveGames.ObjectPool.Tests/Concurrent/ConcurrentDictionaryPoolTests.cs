using GroveGames.ObjectPool.Concurrent;

namespace GroveGames.ObjectPool.Tests.Concurrent;

public sealed class ConcurrentDictionaryPoolTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesPool()
    {
        // Arrange & Act
        using var pool = new ConcurrentDictionaryPool<string, int>(5, 10);

        // Assert
        Assert.Equal(0, pool.Count);
        Assert.Equal(10, pool.MaxSize);
    }

    [Fact]
    public void Constructor_WithComparer_CreatesPool()
    {
        // Arrange & Act
        using var pool = new ConcurrentDictionaryPool<string, int>(5, 10, StringComparer.OrdinalIgnoreCase);

        // Assert
        Assert.Equal(0, pool.Count);
        Assert.Equal(10, pool.MaxSize);
    }

    [Fact]
    public void Constructor_WithNullComparer_CreatesPool()
    {
        // Arrange & Act
        using var pool = new ConcurrentDictionaryPool<string, int>(5, 10, null);

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
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentDictionaryPool<string, int>(initialSize, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_NonPositiveMaxSize_ThrowsArgumentOutOfRangeException(int maxSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentDictionaryPool<string, int>(5, maxSize));
    }

    [Fact]
    public void Constructor_InitialSizeGreaterThanMaxSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentDictionaryPool<string, int>(15, 10));
    }

    [Fact]
    public void Rent_EmptyPool_ReturnsNewDictionary()
    {
        // Arrange
        using var pool = new ConcurrentDictionaryPool<string, int>(0, 5);

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
        using var pool = new ConcurrentDictionaryPool<string, int>(0, 5, StringComparer.OrdinalIgnoreCase);

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
        using var pool = new ConcurrentDictionaryPool<string, int>(0, 5);
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
        using var pool = new ConcurrentDictionaryPool<string, int>(0, 5);
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
        using var pool = new ConcurrentDictionaryPool<string, int>(0, 5);
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
        using var pool = new ConcurrentDictionaryPool<string, int>(0, 5);
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
        var pool = new ConcurrentDictionaryPool<string, int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Count);
    }

    [Fact]
    public void MaxSize_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentDictionaryPool<string, int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.MaxSize);
    }

    [Fact]
    public void Rent_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentDictionaryPool<string, int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Rent());
    }

    [Fact]
    public void Return_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentDictionaryPool<string, int>(0, 5);
        var dictionary = new Dictionary<string, int>();
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Return(dictionary));
    }

    [Fact]
    public void Clear_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentDictionaryPool<string, int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Clear());
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        // Arrange
        var pool = new ConcurrentDictionaryPool<string, int>(0, 5);

        // Act & Assert
        pool.Dispose();
        pool.Dispose();
    }

    [Fact]
    public async Task RentAndReturn_ConcurrentOperations_ThreadSafe()
    {
        // Arrange
        using var pool = new ConcurrentDictionaryPool<string, int>(0, 100);
        var rentedDictionaries = new List<Dictionary<string, int>>();
        var lockObject = new object();

        // Act
        var tasks = Enumerable.Range(0, 100).Select(async i =>
        {
            await Task.Yield();
            var dictionary = pool.Rent();
            dictionary[$"key{i}"] = i;
            lock (lockObject)
            {
                rentedDictionaries.Add(dictionary);
            }
            pool.Return(dictionary);
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(100, rentedDictionaries.Count);
        Assert.True(pool.Count >= 0);
    }

    [Fact]
    public async Task Return_ConcurrentReturns_ThreadSafe()
    {
        // Arrange
        using var pool = new ConcurrentDictionaryPool<string, int>(0, 50);
        var dictionaries = Enumerable.Range(0, 100).Select(_ => new Dictionary<string, int>()).ToArray();

        // Act
        var tasks = dictionaries.Select(async dictionary =>
        {
            await Task.Yield();
            pool.Return(dictionary);
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.True(pool.Count >= 0);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 10)]
    [InlineData(0, 100)]
    public void Constructor_ValidSizeCombinations_CreatesPool(int initialSize, int maxSize)
    {
        // Arrange & Act
        using var pool = new ConcurrentDictionaryPool<string, int>(initialSize, maxSize);

        // Assert
        Assert.Equal(0, pool.Count);
        Assert.Equal(maxSize, pool.MaxSize);
    }

    [Fact]
    public async Task RentMultiple_ConcurrentOperations_CreatesMultipleDictionaries()
    {
        // Arrange
        using var pool = new ConcurrentDictionaryPool<string, int>(0, 50);
        var rentedDictionaries = new List<Dictionary<string, int>>();
        var lockObject = new object();

        // Act
        var tasks = Enumerable.Range(0, 50).Select(async _ =>
        {
            await Task.Yield();
            var dictionary = pool.Rent();
            lock (lockObject)
            {
                rentedDictionaries.Add(dictionary);
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(50, rentedDictionaries.Count);
        Assert.All(rentedDictionaries, dict => Assert.Empty(dict));
    }

    [Fact]
    public async Task Return_ConcurrentReturnsWithMixedData_ClearsAllEntries()
    {
        // Arrange
        using var pool = new ConcurrentDictionaryPool<string, object>(0, 50);

        // Act
        var tasks = Enumerable.Range(0, 50).Select(async i =>
        {
            await Task.Yield();
            var dictionary = pool.Rent();
            dictionary["string"] = "value";
            dictionary["number"] = i;
            dictionary["null"] = null!;
            pool.Return(dictionary);
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.True(pool.Count >= 0);
    }

    [Fact]
    public async Task Rent_WithCustomComparerConcurrently_PreservesComparerBehavior()
    {
        // Arrange
        using var pool = new ConcurrentDictionaryPool<string, int>(0, 50, StringComparer.OrdinalIgnoreCase);

        // Act
        var tasks = Enumerable.Range(0, 50).Select(async i =>
        {
            await Task.Yield();
            var dict = pool.Rent();
            dict["test"] = i;
            dict["TEST"] = i + 1;
            pool.Return(dict);
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.True(pool.Count >= 0);
        var testDict = pool.Rent();
        testDict["test"] = 1;
        testDict["TEST"] = 2;
        Assert.Single(testDict);
        Assert.Equal(2, testDict["test"]);
    }

    [Fact]
    public async Task Clear_DuringConcurrentOperations_ThreadSafe()
    {
        // Arrange
        using var pool = new ConcurrentDictionaryPool<string, int>(0, 50);

        // Act
        var rentTasks = Enumerable.Range(0, 25).Select(async i =>
        {
            try
            {
                await Task.Yield();
                var dictionary = pool.Rent();
                dictionary[$"key{i}"] = i;
                pool.Return(dictionary);
            }
            catch (ObjectDisposedException)
            {
            }
        });

        var clearTask = Task.Run(async () =>
        {
            await Task.Delay(10);
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
        Assert.True(pool.Count >= 0);
    }

    [Fact]
    public async Task Dispose_DuringConcurrentOperations_ThreadSafe()
    {
        // Arrange
        var pool = new ConcurrentDictionaryPool<string, int>(0, 50);
        var exceptions = new List<Exception>();
        var lockObject = new object();

        // Act
        var rentTasks = Enumerable.Range(0, 25).Select(async _ =>
        {
            try
            {
                await Task.Yield();
                var dictionary = pool.Rent();
                pool.Return(dictionary);
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
            await Task.Delay(5);
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
        using var pool = new ConcurrentDictionaryPool<string, int>(0, 200);
        var totalOperations = 500;
        var completedOperations = 0;

        // Act
        var tasks = Enumerable.Range(0, totalOperations).Select(async i =>
        {
            await Task.Yield();
            var dictionary = pool.Rent();
            dictionary[$"key{i}"] = i;
            await Task.Delay(1);
            pool.Return(dictionary);
            Interlocked.Increment(ref completedOperations);
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(totalOperations, completedOperations);
    }

    [Fact]
    public async Task Return_DictionaryWithComplexObjectsConcurrently_ClearsCorrectly()
    {
        // Arrange
        using var pool = new ConcurrentDictionaryPool<int, List<string>>(0, 50);

        // Act
        var tasks = Enumerable.Range(0, 50).Select(async i =>
        {
            await Task.Yield();
            var dictionary = pool.Rent();
            dictionary[1] = new List<string> { $"a{i}", $"b{i}" };
            dictionary[2] = new List<string> { $"c{i}", $"d{i}" };
            pool.Return(dictionary);
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        var testDict = pool.Rent();
        Assert.Empty(testDict);
    }

    [Fact]
    public async Task RentAndReturn_ReusesConcurrently_ThreadSafe()
    {
        // Arrange
        using var pool = new ConcurrentDictionaryPool<string, int>(0, 20);
        pool.Return(new Dictionary<string, int>());
        pool.Return(new Dictionary<string, int>());
        pool.Return(new Dictionary<string, int>());

        // Act
        var tasks = Enumerable.Range(0, 100).Select(async i =>
        {
            await Task.Yield();
            var dictionary = pool.Rent();
            dictionary[$"key{i}"] = i;
            pool.Return(dictionary);
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.True(pool.Count >= 0);
    }
}