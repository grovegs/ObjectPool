using System.Collections.Concurrent;

using GroveGames.ObjectPool.Concurrent;

namespace GroveGames.ObjectPool.Tests.Concurrent;

public sealed class ConcurrentTypedObjectPoolTests
{
    private abstract class TypedTestBase
    {
        public string Id { get; set; } = string.Empty;
        public bool WasRented { get; set; }
        public bool WasReturned { get; set; }
    }

    private sealed class TypedTestDerived : TypedTestBase
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void Constructor_ValidParameters_CreatesPool()
    {
        // Arrange & Act
        using var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(), null, null, 1, 5);

        // Assert
        Assert.Equal(5, pool.MaxSize);
    }

    [Fact]
    public void Constructor_NullFactory_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(null!, null, null, 1, 5));
    }

    [Fact]
    public void Constructor_NegativeInitialSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(() => new TypedTestDerived(), null, null, -1, 5));
    }

    [Fact]
    public void Constructor_InitialSizeGreaterThanMaxSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(() => new TypedTestDerived(), null, null, 10, 5));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidMaxSize_ThrowsArgumentOutOfRangeException(int maxSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(() => new TypedTestDerived(), null, null, 1, maxSize));
    }

    [Fact]
    public void Constructor_WithCallbacks_CreatesPool()
    {
        // Arrange & Act
        using var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(),
            item => item.WasRented = true,
            item => item.WasReturned = true,
            1, 5);

        // Assert
        Assert.Equal(5, pool.MaxSize);
    }

    [Fact]
    public void Rent_ValidPool_ReturnsBaseType()
    {
        // Arrange
        using var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived { Value = 42 }, null, null, 1, 5);

        // Act
        var item = pool.Rent();

        // Assert
        Assert.NotNull(item);
        Assert.IsType<TypedTestDerived>(item);
        Assert.IsAssignableFrom<TypedTestBase>(item);
        Assert.Equal(42, ((TypedTestDerived)item).Value);
    }

    [Fact]
    public void Rent_WithOnRentCallback_CallsCallback()
    {
        // Arrange
        using var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(),
            item => item.WasRented = true,
            null,
            1, 5);

        // Act
        var item = pool.Rent();

        // Assert
        Assert.True(((TypedTestDerived)item).WasRented);
    }

    [Fact]
    public void Rent_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(), null, null, 1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Rent());
    }

    [Fact]
    public void Return_ValidItem_DoesNotThrow()
    {
        // Arrange
        using var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(), null, null, 1, 5);
        var item = pool.Rent();

        // Act & Assert
        pool.Return(item);
    }

    [Fact]
    public void Return_WithOnReturnCallback_CallsCallback()
    {
        // Arrange
        using var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(),
            null,
            item => item.WasReturned = true,
            1, 5);
        var item = pool.Rent();

        // Act
        pool.Return(item);

        // Assert
        Assert.True(((TypedTestDerived)item).WasReturned);
    }

    [Fact]
    public void Return_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(), null, null, 1, 5);
        var item = pool.Rent();
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Return(item));
    }

    [Fact]
    public void Clear_ValidPool_DoesNotThrow()
    {
        // Arrange
        using var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(), null, null, 1, 5);

        // Act & Assert
        pool.Clear();
    }

    [Fact]
    public void Clear_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(), null, null, 1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Clear());
    }

    [Fact]
    public void Count_ValidPool_ReturnsValue()
    {
        // Arrange
        using var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(), null, null, 2, 5);

        // Act
        var count = pool.Count;

        // Assert
        Assert.True(count >= 0);
    }

    [Fact]
    public void Count_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(), null, null, 1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Count);
    }

    [Fact]
    public void MaxSize_ValidPool_ReturnsExpectedValue()
    {
        // Arrange
        using var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(), null, null, 1, 15);

        // Act
        var maxSize = pool.MaxSize;

        // Assert
        Assert.Equal(15, maxSize);
    }

    [Fact]
    public void MaxSize_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(), null, null, 1, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.MaxSize);
    }

    [Fact]
    public void Dispose_CalledOnce_DoesNotThrow()
    {
        // Arrange
        var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(), null, null, 1, 5);

        // Act & Assert
        pool.Dispose();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(), null, null, 1, 5);

        // Act & Assert
        pool.Dispose();
        pool.Dispose();
        pool.Dispose();
    }

    [Fact]
    public void RentReturn_BothCallbacks_BothCalled()
    {
        // Arrange
        using var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(),
            item => item.WasRented = true,
            item => item.WasReturned = true,
            1, 5);

        // Act
        var item = pool.Rent();
        pool.Return(item);

        // Assert
        var derivedItem = (TypedTestDerived)item;
        Assert.True(derivedItem.WasRented);
        Assert.True(derivedItem.WasReturned);
    }

    [Fact]
    public void RentReturn_FactoryCalledCorrectly_CreatesInstances()
    {
        // Arrange
        var callCount = 0;
        using var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () =>
            {
                callCount++;
                return new TypedTestDerived { Value = callCount };
            },
            null, null, 0, 5);

        // Act
        var item1 = pool.Rent();
        var item2 = pool.Rent();

        // Assert
        Assert.True(callCount >= 1);
        Assert.IsType<TypedTestDerived>(item1);
        Assert.IsType<TypedTestDerived>(item2);
    }

    [Fact]
    public void RentReturn_TypeCasting_WorksCorrectly()
    {
        // Arrange
        using var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived { Name = "Test", Value = 99 }, null, null, 1, 5);

        // Act
        var baseItem = pool.Rent();
        var derivedItem = (TypedTestDerived)baseItem;

        // Assert
        Assert.Equal("Test", derivedItem.Name);
        Assert.Equal(99, derivedItem.Value);

        pool.Return(baseItem);
    }

    [Fact]
    public async Task Rent_ConcurrentAccess_AllSucceed()
    {
        // Arrange
        using var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(), null, null, 10, 50);
        var results = new ConcurrentBag<TypedTestBase>();

        // Act
        var tasks = Enumerable.Range(0, 20).Select(_ => Task.Run(() =>
        {
            var item = pool.Rent();
            results.Add(item);
            return item;
        })).ToArray();

        var items = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(20, results.Count);
        Assert.All(items, item => Assert.NotNull(item));
        Assert.All(items, item => Assert.IsType<TypedTestDerived>(item));

        foreach (var item in items)
        {
            pool.Return(item);
        }
    }

    [Fact]
    public async Task RentReturn_ConcurrentOperations_AllSucceed()
    {
        // Arrange
        using var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(), null, null, 5, 20);

        // Act
        var tasks = Enumerable.Range(0, 50).Select(i => Task.Run(() =>
        {
            var item = pool.Rent();
            ((TypedTestDerived)item).Value = i;
            pool.Return(item);
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert - No exceptions thrown
    }

    [Fact]
    public async Task RentReturn_ConcurrentWithCallbacks_AllSucceed()
    {
        // Arrange
        var rentCount = 0;
        var returnCount = 0;
        using var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(),
            _ => Interlocked.Increment(ref rentCount),
            _ => Interlocked.Increment(ref returnCount),
            5, 20);

        // Act
        var tasks = Enumerable.Range(0, 30).Select(_ => Task.Run(() =>
        {
            var item = pool.Rent();
            pool.Return(item);
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(30, rentCount);
        Assert.Equal(30, returnCount);
    }

    [Fact]
    public async Task Dispose_ConcurrentWithOperations_HandlesGracefully()
    {
        // Arrange
        var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(), null, null, 5, 20);
        var exceptions = new ConcurrentBag<Exception>();

        // Act
        var operationTasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            try
            {
                var item = pool.Rent();
                pool.Return(item);
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

        // Assert - Some operations may throw ObjectDisposedException, which is expected
        Assert.All(exceptions, ex => Assert.IsType<ObjectDisposedException>(ex));
    }

    [Fact]
    public void RentReturn_SameInstance_CallbacksCalledMultipleTimes()
    {
        // Arrange
        using var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(),
            item => item.Value++,
            item => item.Name += "R",
            1, 5);

        // Act
        var item1 = pool.Rent();
        pool.Return(item1);
        var item2 = pool.Rent();
        pool.Return(item2);

        // Assert
        var derivedItem = (TypedTestDerived)item2;
        Assert.Equal(2, derivedItem.Value);
        Assert.Equal("RR", derivedItem.Name);
    }

    [Fact]
    public void Pool_ImplementsIObjectPoolInterface_Correctly()
    {
        // Arrange
        using var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(), null, null, 1, 5);
        IObjectPool<TypedTestBase> interfacePool = pool;

        // Act
        var item = interfacePool.Rent();
        var count = interfacePool.Count;
        var maxSize = interfacePool.MaxSize;

        // Assert
        Assert.NotNull(item);
        Assert.True(count >= 0);
        Assert.Equal(5, maxSize);

        interfacePool.Return(item);
        interfacePool.Clear();
    }

    [Fact]
    public void RentReturn_OnlyOnRentCallback_WorksCorrectly()
    {
        // Arrange
        using var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(),
            item => item.WasRented = true,
            null,
            1, 5);

        // Act
        var item = pool.Rent();
        pool.Return(item);

        // Assert
        var derivedItem = (TypedTestDerived)item;
        Assert.True(derivedItem.WasRented);
        Assert.False(derivedItem.WasReturned);
    }

    [Fact]
    public void RentReturn_OnlyOnReturnCallback_WorksCorrectly()
    {
        // Arrange
        using var pool = new ConcurrentTypedObjectPool<TypedTestBase, TypedTestDerived>(
            () => new TypedTestDerived(),
            null,
            item => item.WasReturned = true,
            1, 5);

        // Act
        var item = pool.Rent();
        pool.Return(item);

        // Assert
        var derivedItem = (TypedTestDerived)item;
        Assert.False(derivedItem.WasRented);
        Assert.True(derivedItem.WasReturned);
    }
}