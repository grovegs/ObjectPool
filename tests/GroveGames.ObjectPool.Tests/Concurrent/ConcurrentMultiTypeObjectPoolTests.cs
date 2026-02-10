using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using GroveGames.ObjectPool.Concurrent;

namespace GroveGames.ObjectPool.Tests.Concurrent;

public sealed class ConcurrentMultiTypeObjectPoolTests
{
    private abstract class TestBase
    {
        public string Id { get; set; } = string.Empty;
    }

    private sealed class TestDerived1 : TestBase
    {
        public int Value { get; set; }
    }

    private sealed class TestDerived2 : TestBase
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestDerived3 : TestBase
    {
        public bool Flag { get; set; }
    }

    [Fact]
    public void Constructor_NullConfigure_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ConcurrentMultiTypeObjectPool<TestBase>(null!));
    }

    [Fact]
    public void Constructor_ValidConfiguration_CreatesPool()
    {
        // Arrange & Act
        using var pool = new ConcurrentMultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 1, 5);
        });

        // Assert
        Assert.Equal(5, pool.MaxSize<TestDerived1>());
    }

    [Fact]
    public void Count_RegisteredType_ReturnsValue()
    {
        // Arrange
        using var pool = new ConcurrentMultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 2, 10);
        });

        // Act
        var count = pool.Count<TestDerived1>();

        // Assert
        Assert.True(count >= 0);
    }

    [Fact]
    public void Count_UnregisteredType_ReturnsZero()
    {
        // Arrange
        using var pool = new ConcurrentMultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 1, 5);
        });

        // Act
        var count = pool.Count<TestDerived2>();

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public void Count_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentMultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 1, 5);
        });
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Count<TestDerived1>());
    }

    [Fact]
    public void MaxSize_RegisteredType_ReturnsExpectedValue()
    {
        // Arrange
        using var pool = new ConcurrentMultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 1, 15);
        });

        // Act
        var maxSize = pool.MaxSize<TestDerived1>();

        // Assert
        Assert.Equal(15, maxSize);
    }

    [Fact]
    public void MaxSize_UnregisteredType_ReturnsZero()
    {
        // Arrange
        using var pool = new ConcurrentMultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 1, 5);
        });

        // Act
        var maxSize = pool.MaxSize<TestDerived2>();

        // Assert
        Assert.Equal(0, maxSize);
    }

    [Fact]
    public void MaxSize_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentMultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 1, 5);
        });
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.MaxSize<TestDerived1>());
    }

    [Fact]
    public void Rent_RegisteredType_ReturnsInstance()
    {
        // Arrange
        using var pool = new ConcurrentMultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1 { Value = 42 }, null, null, 1, 5);
        });

        // Act
        var item = pool.Rent<TestDerived1>();

        // Assert
        Assert.NotNull(item);
        Assert.IsType<TestDerived1>(item);
    }

    [Fact]
    public void Rent_UnregisteredType_ThrowsInvalidOperationException()
    {
        // Arrange
        using var pool = new ConcurrentMultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 1, 5);
        });

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => pool.Rent<TestDerived2>());
    }

    [Fact]
    public void Rent_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentMultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 1, 5);
        });
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Rent<TestDerived1>());
    }

    [Fact]
    public void Return_RegisteredType_DoesNotThrow()
    {
        // Arrange
        using var pool = new ConcurrentMultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 1, 5);
        });
        var item = pool.Rent<TestDerived1>();

        // Act & Assert
        pool.Return(item);
    }

    [Fact]
    public void Return_UnregisteredType_DoesNotThrow()
    {
        // Arrange
        using var pool = new ConcurrentMultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 1, 5);
        });
        var item = new TestDerived2();

        // Act & Assert
        pool.Return(item);
    }

    [Fact]
    public void Return_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentMultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 1, 5);
        });
        var item = pool.Rent<TestDerived1>();
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Return(item));
    }

    [Fact]
    public void Clear_ValidPool_DoesNotThrow()
    {
        // Arrange
        using var pool = new ConcurrentMultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 1, 5);
            builder.AddPool<TestDerived2>(() => new TestDerived2(), null, null, 1, 5);
        });

        // Act & Assert
        pool.Clear();
    }

    [Fact]
    public void Clear_DisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ConcurrentMultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 1, 5);
        });
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Clear());
    }

    [Fact]
    public void Dispose_CalledOnce_DoesNotThrow()
    {
        // Arrange
        var pool = new ConcurrentMultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 1, 5);
        });

        // Act & Assert
        pool.Dispose();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var pool = new ConcurrentMultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 1, 5);
        });

        // Act & Assert
        pool.Dispose();
        pool.Dispose();
        pool.Dispose();
    }

    [Fact]
    public void MultipleTypes_RentReturn_WorksCorrectly()
    {
        // Arrange
        using var pool = new ConcurrentMultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 1, 5);
            builder.AddPool<TestDerived2>(() => new TestDerived2(), null, null, 1, 5);
            builder.AddPool<TestDerived3>(() => new TestDerived3(), null, null, 1, 5);
        });

        // Act
        var item1 = pool.Rent<TestDerived1>();
        var item2 = pool.Rent<TestDerived2>();
        var item3 = pool.Rent<TestDerived3>();

        // Assert
        Assert.IsType<TestDerived1>(item1);
        Assert.IsType<TestDerived2>(item2);
        Assert.IsType<TestDerived3>(item3);

        pool.Return(item1);
        pool.Return(item2);
        pool.Return(item3);
    }

    [Fact]
    public async Task Rent_ConcurrentAccessSameType_AllSucceed()
    {
        // Arrange
        using var pool = new ConcurrentMultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 10, 50);
        });
        var results = new ConcurrentBag<TestBase>();

        // Act
        var tasks = Enumerable.Range(0, 20).Select(_ => Task.Run(() =>
        {
            var item = pool.Rent<TestDerived1>();
            results.Add(item);
            return item;
        })).ToArray();

        var items = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(20, results.Count);
        Assert.All(items, item => Assert.NotNull(item));
        Assert.All(items, item => Assert.IsType<TestDerived1>(item));

        foreach (var item in items)
        {
            pool.Return((TestDerived1)item);
        }
    }

    [Fact]
    public async Task Rent_ConcurrentAccessDifferentTypes_AllSucceed()
    {
        // Arrange
        using var pool = new ConcurrentMultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 5, 25);
            builder.AddPool<TestDerived2>(() => new TestDerived2(), null, null, 5, 25);
        });
        var results1 = new ConcurrentBag<TestBase>();
        var results2 = new ConcurrentBag<TestBase>();

        // Act
        var tasks1 = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            var item = pool.Rent<TestDerived1>();
            results1.Add(item);
            return item;
        }));

        var tasks2 = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            var item = pool.Rent<TestDerived2>();
            results2.Add(item);
            return item;
        }));

        var allTasks = tasks1.Concat(tasks2).ToArray();
        await Task.WhenAll(allTasks);

        // Assert
        Assert.Equal(10, results1.Count);
        Assert.Equal(10, results2.Count);

        foreach (var item in results1)
        {
            pool.Return(item);
        }
        foreach (var item in results2)
        {
            pool.Return(item);
        }
    }

    [Fact]
    public async Task RentReturn_ConcurrentOperations_AllSucceed()
    {
        // Arrange
        using var pool = new ConcurrentMultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 3, 15);
            builder.AddPool<TestDerived2>(() => new TestDerived2(), null, null, 3, 15);
        });

        // Act
        var tasks = Enumerable.Range(0, 30).Select(i => Task.Run(() =>
        {
            if (i % 2 == 0)
            {
                var item = pool.Rent<TestDerived1>();
                pool.Return(item);
            }
            else
            {
                var item = pool.Rent<TestDerived2>();
                pool.Return(item);
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert - No exceptions thrown
    }

    [Fact]
    public async Task Dispose_ConcurrentWithOperations_HandlesGracefully()
    {
        // Arrange
        var pool = new ConcurrentMultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 3, 15);
        });
        var exceptions = new ConcurrentBag<Exception>();

        // Act
        var operationTasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            try
            {
                var item = pool.Rent<TestDerived1>();
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
    public void RentReturn_SameTypeMultipleTimes_WorksCorrectly()
    {
        // Arrange
        using var pool = new ConcurrentMultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 1, 5);
        });

        // Act & Assert
        for (int i = 0; i < 10; i++)
        {
            var item = pool.Rent<TestDerived1>();
            Assert.NotNull(item);
            pool.Return(item);
        }
    }

    [Fact]
    public void EmptyConfiguration_CreatesEmptyPool()
    {
        // Arrange & Act
        using var pool = new ConcurrentMultiTypeObjectPool<TestBase>(_ => { });

        // Act & Assert
        Assert.Equal(0, pool.Count<TestDerived1>());
        Assert.Equal(0, pool.MaxSize<TestDerived1>());
        Assert.Throws<InvalidOperationException>(() => pool.Rent<TestDerived1>());
    }
}
