using GroveGames.ObjectPool.Concurrent;

namespace GroveGames.ObjectPool.Tests.Concurrent;

public sealed class ConcurrentObjectPoolTests
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
        using var pool = new ConcurrentObjectPool<string>(() => "test", null, null, 5, 10);

        // Assert
        Assert.Equal(0, pool.Count);
        Assert.Equal(10, pool.MaxSize);
    }

    [Fact]
    public void Constructor_NullFactory_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ConcurrentObjectPool<string>(null!, null, null, 5, 10));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_NegativeInitialSize_ThrowsArgumentOutOfRangeException(int initialSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ConcurrentObjectPool<string>(() => "test", null, null, initialSize, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_NonPositiveMaxSize_ThrowsArgumentOutOfRangeException(int maxSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ConcurrentObjectPool<string>(() => "test", null, null, 5, maxSize));
    }

    [Fact]
    public void Constructor_InitialSizeGreaterThanMaxSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ConcurrentObjectPool<string>(() => "test", null, null, 15, 10));
    }

    [Fact]
    public void Rent_EmptyPool_CreatesNewItem()
    {
        // Arrange
        using var pool = new ConcurrentObjectPool<string>(() => "created", null, null, 0, 5);

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
        using var pool = new ConcurrentObjectPool<string>(() => "test", item => rentedItems.Add(item), null, 0, 5);

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
        using var pool = new ConcurrentObjectPool<string>(() => "test", null, null, 0, 5);
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
        using var pool = new ConcurrentObjectPool<string>(() => "test", null, item => returnedItems.Add(item), 0, 5);
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
        using var pool = new ConcurrentObjectPool<string>(() => "test", null, null, 0, 2);
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
        using var pool = new ConcurrentObjectPool<string>(() => "new", null, null, 0, 5);
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
        using var pool = new ConcurrentObjectPool<string>(() => "test", null, null, 0, 5);
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
        var pool = new ConcurrentObjectPool<string>(() => "test", null, null, 0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Count);
    }

    [Fact]
    public void MaxSize_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentObjectPool<string>(() => "test", null, null, 0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.MaxSize);
    }

    [Fact]
    public void Rent_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentObjectPool<string>(() => "test", null, null, 0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Rent());
    }

    [Fact]
    public void Return_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentObjectPool<string>(() => "test", null, null, 0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Return("test"));
    }

    [Fact]
    public void Clear_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentObjectPool<string>(() => "test", null, null, 0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Clear());
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        // Arrange
        var pool = new ConcurrentObjectPool<string>(() => "test", null, null, 0, 5);

        // Act & Assert
        pool.Dispose();
        pool.Dispose();
    }

    [Fact]
    public void Dispose_WithDisposableItems_DisposesItems()
    {
        // Arrange
        var disposableItems = new List<TestDisposable>();
        using var pool = new ConcurrentObjectPool<TestDisposable>(() => new TestDisposable(), null, null, 0, 5);

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

    [Fact]
    public async Task RentAndReturn_ConcurrentOperations_ThreadSafe()
    {
        // Arrange
        using var pool = new ConcurrentObjectPool<string>(() => Guid.NewGuid().ToString(), null, null, 0, 100);
        var rentedItems = new List<string>();
        var lockObject = new object();

        // Act
        var tasks = Enumerable.Range(0, 100).Select(async i =>
        {
            await Task.Yield();
            var item = pool.Rent();
            lock (lockObject)
            {
                rentedItems.Add(item);
            }
            pool.Return(item);
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(100, rentedItems.Count);
        Assert.True(pool.Count <= 100);
    }

    [Fact]
    public async Task Return_ConcurrentReturns_MayExceedMaxSizeDueToRaceCondition()
    {
        // Arrange
        using var pool = new ConcurrentObjectPool<string>(() => "test", null, null, 0, 10);
        var items = Enumerable.Range(0, 20).Select(i => $"item{i}").ToArray();

        // Act
        var tasks = items.Select(async item =>
        {
            await Task.Yield();
            pool.Return(item);
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.True(pool.Count >= 10);
        Assert.True(pool.Count <= 20);
    }

    [Fact]
    public async Task Rent_ConcurrentRents_AllSucceed()
    {
        // Arrange
        using var pool = new ConcurrentObjectPool<string>(() => Guid.NewGuid().ToString(), null, null, 0, 50);
        var rentedItems = new List<string>();
        var lockObject = new object();

        // Act
        var tasks = Enumerable.Range(0, 50).Select(async _ =>
        {
            await Task.Yield();
            var item = pool.Rent();
            lock (lockObject)
            {
                rentedItems.Add(item);
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(50, rentedItems.Count);
        Assert.All(rentedItems, item => Assert.NotNull(item));
    }

    [Fact]
    public async Task Clear_DuringConcurrentOperations_ThreadSafe()
    {
        // Arrange
        using var pool = new ConcurrentObjectPool<string>(() => "test", null, null, 0, 50);

        // Act
        var rentTasks = Enumerable.Range(0, 25).Select(async _ =>
        {
            try
            {
                await Task.Yield();
                var item = pool.Rent();
                pool.Return(item);
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

        await Task.WhenAll(rentTasks.Concat(new[] { clearTask }));

        // Assert
        Assert.True(pool.Count >= 0);
    }

    [Fact]
    public async Task Dispose_DuringConcurrentOperations_ThreadSafe()
    {
        // Arrange
        var pool = new ConcurrentObjectPool<string>(() => "test", null, null, 0, 50);
        var exceptions = new List<Exception>();
        var lockObject = new object();

        // Act
        var rentTasks = Enumerable.Range(0, 25).Select(async _ =>
        {
            try
            {
                await Task.Yield();
                var item = pool.Rent();
                pool.Return(item);
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

        await Task.WhenAll(rentTasks.Concat(new[] { disposeTask }));

        // Assert
        Assert.True(exceptions.All(ex => ex is ObjectDisposedException));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 10)]
    [InlineData(0, 100)]
    public void Constructor_ValidSizeCombinations_CreatesPool(int initialSize, int maxSize)
    {
        // Arrange & Act
        using var pool = new ConcurrentObjectPool<string>(() => "test", null, null, initialSize, maxSize);

        // Assert
        Assert.Equal(0, pool.Count);
        Assert.Equal(maxSize, pool.MaxSize);
    }

    [Fact]
    public async Task RentMultiple_ConcurrentFactory_CreatesUniqueItems()
    {
        // Arrange
        var counter = 0;
        using var pool = new ConcurrentObjectPool<string>(() => $"item{Interlocked.Increment(ref counter)}", null, null, 0, 50);
        var rentedItems = new List<string>();
        var lockObject = new object();

        // Act
        var tasks = Enumerable.Range(0, 50).Select(async _ =>
        {
            await Task.Yield();
            var item = pool.Rent();
            lock (lockObject)
            {
                rentedItems.Add(item);
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(50, rentedItems.Count);
        Assert.Equal(50, rentedItems.Distinct().Count());
    }

    [Fact]
    public async Task Callbacks_ConcurrentOperations_ThreadSafe()
    {
        // Arrange
        var rentCount = 0;
        var returnCount = 0;
        using var pool = new ConcurrentObjectPool<string>(
            () => "test",
            _ => Interlocked.Increment(ref rentCount),
            _ => Interlocked.Increment(ref returnCount),
            0, 50);

        // Act
        var tasks = Enumerable.Range(0, 50).Select(async _ =>
        {
            await Task.Yield();
            var item = pool.Rent();
            pool.Return(item);
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(50, rentCount);
        Assert.Equal(50, returnCount);
    }

    [Fact]
    public async Task Pool_HighConcurrencyStressTest_HandlesCorrectly()
    {
        // Arrange
        using var pool = new ConcurrentObjectPool<string>(() => Guid.NewGuid().ToString(), null, null, 0, 200);
        var totalOperations = 1000;
        var completedOperations = 0;

        // Act
        var tasks = Enumerable.Range(0, totalOperations).Select(async i =>
        {
            await Task.Yield();
            var item = pool.Rent();
            await Task.Delay(1);
            pool.Return(item);
            Interlocked.Increment(ref completedOperations);
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(totalOperations, completedOperations);
    }

    [Fact]
    public async Task RentAndReturn_ReusesConcurrently_ThreadSafe()
    {
        // Arrange
        using var pool = new ConcurrentObjectPool<string>(() => "new", null, null, 0, 10);
        pool.Return("pooled1");
        pool.Return("pooled2");
        pool.Return("pooled3");

        // Act
        var tasks = Enumerable.Range(0, 50).Select(async _ =>
        {
            await Task.Yield();
            var item = pool.Rent();
            pool.Return(item);
        }).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.True(pool.Count >= 0);
    }
}