using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GroveGames.ObjectPool.Concurrent;

namespace GroveGames.ObjectPool.Tests.Concurrent;

public sealed class ConcurrentKeyedObjectPoolTests
{
    private sealed class TestObject
    {
        public string Key { get; set; } = string.Empty;
        public bool IsRented { get; set; }
        public bool IsReturned { get; set; }
    }

    private sealed class TestPool : IConcurrentObjectPool<TestObject>
    {
        public bool IsDisposed { get; private set; }

        public int Count => 0;

        public int MaxSize => 1;

        public TestObject Rent()
        {
            return new TestObject();
        }

        public void Return(TestObject item)
        {
        }

        public void Clear()
        {
        }

        public void Warm()
        {
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    [Fact]
    public void Constructor_NullFactory_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ConcurrentKeyedObjectPool<string, TestObject>(null!));
    }

    [Fact]
    public void Count_UnusedKey_ReturnsZero()
    {
        using var pool = new ConcurrentKeyedObjectPool<string, TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 5, 10));

        Assert.Equal(0, pool.Count("a"));
    }

    [Fact]
    public void MaxSize_UnusedKey_ReturnsZero()
    {
        using var pool = new ConcurrentKeyedObjectPool<string, TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 5, 10));

        Assert.Equal(0, pool.MaxSize("a"));
    }

    [Fact]
    public void MaxSize_UsedKey_ReturnsPoolMaxSize()
    {
        using var pool = new ConcurrentKeyedObjectPool<string, TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 15));

        pool.Rent("a");

        Assert.Equal(15, pool.MaxSize("a"));
    }

    [Fact]
    public void Rent_NewKey_ReturnsItemFromKeyPool()
    {
        using var pool = new ConcurrentKeyedObjectPool<string, TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 5));

        var item = pool.Rent("b");

        Assert.Equal("b", item.Key);
    }

    [Fact]
    public void Rent_SameKeyTwice_InvokesFactoryOnce()
    {
        var factoryCalls = 0;
        using var pool = new ConcurrentKeyedObjectPool<string, TestObject>(key =>
        {
            factoryCalls++;
            return new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 5);
        });

        pool.Rent("a");
        pool.Rent("a");

        Assert.Equal(1, factoryCalls);
    }

    [Fact]
    public void Rent_DifferentKeys_UsesSeparatePools()
    {
        using var pool = new ConcurrentKeyedObjectPool<string, TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 5));

        var first = pool.Rent("a");
        var second = pool.Rent("b");
        pool.Return("a", first);

        Assert.Equal("a", first.Key);
        Assert.Equal("b", second.Key);
        Assert.Equal(1, pool.Count("a"));
        Assert.Equal(0, pool.Count("b"));
    }

    [Fact]
    public void Rent_FactoryReturnsNull_ThrowsArgumentNullException()
    {
        using var pool = new ConcurrentKeyedObjectPool<string, TestObject>(_ => null!);

        Assert.Throws<ArgumentNullException>(() => pool.Rent("a"));
    }

    [Fact]
    public void Rent_WithOnRentCallback_InvokesCallback()
    {
        using var pool = new ConcurrentKeyedObjectPool<string, TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, static item => item.IsRented = true, null, 0, 5));

        var item = pool.Rent("a");

        Assert.True(item.IsRented);
    }

    [Fact]
    public void Return_ItemToPool_AddsToPool()
    {
        using var pool = new ConcurrentKeyedObjectPool<string, TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 5));
        var item = pool.Rent("a");

        pool.Return("a", item);

        Assert.Equal(1, pool.Count("a"));
    }

    [Fact]
    public void Return_WithOnReturnCallback_InvokesCallback()
    {
        using var pool = new ConcurrentKeyedObjectPool<string, TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, static item => item.IsReturned = true, 0, 5));
        var item = pool.Rent("a");

        pool.Return("a", item);

        Assert.True(item.IsReturned);
    }

    [Fact]
    public void Return_UnusedKey_CreatesPoolAndStoresItem()
    {
        using var pool = new ConcurrentKeyedObjectPool<string, TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 5));

        pool.Return("c", new TestObject { Key = "c" });

        Assert.Equal(1, pool.Count("c"));
    }

    [Fact]
    public void Warm_Key_PreAllocatesItems()
    {
        using var pool = new ConcurrentKeyedObjectPool<string, TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 5, 10));

        pool.Warm("a");

        Assert.Equal(5, pool.Count("a"));
        Assert.Equal(0, pool.Count("b"));
    }

    [Fact]
    public void Warm_NoParameters_WarmsCreatedPools()
    {
        using var pool = new ConcurrentKeyedObjectPool<string, TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 5, 10));
        pool.Rent("a");
        pool.Rent("b");

        pool.Warm();

        Assert.Equal(5, pool.Count("a"));
        Assert.Equal(5, pool.Count("b"));
        Assert.Equal(0, pool.Count("c"));
    }

    [Fact]
    public void Clear_Key_ClearsOnlyThatPool()
    {
        using var pool = new ConcurrentKeyedObjectPool<string, TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 5, 10));
        pool.Warm("a");
        pool.Warm("b");

        pool.Clear("a");

        Assert.Equal(0, pool.Count("a"));
        Assert.Equal(5, pool.Count("b"));
    }

    [Fact]
    public void Clear_UnusedKey_DoesNotCreatePool()
    {
        var factoryCalls = 0;
        using var pool = new ConcurrentKeyedObjectPool<string, TestObject>(key =>
        {
            factoryCalls++;
            return new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 5);
        });

        pool.Clear("a");

        Assert.Equal(0, factoryCalls);
    }

    [Fact]
    public void Clear_NoParameters_ClearsAllPools()
    {
        using var pool = new ConcurrentKeyedObjectPool<string, TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 5, 10));
        pool.Warm("a");
        pool.Warm("b");

        pool.Clear();

        Assert.Equal(0, pool.Count("a"));
        Assert.Equal(0, pool.Count("b"));
    }

    [Fact]
    public void Dispose_CreatedPools_DisposesEachPool()
    {
        var created = new TestPool[2];
        var index = 0;
        var pool = new ConcurrentKeyedObjectPool<string, TestObject>(_ => created[index++] = new TestPool());
        pool.Rent("a");
        pool.Rent("b");

        pool.Dispose();

        Assert.True(created[0].IsDisposed);
        Assert.True(created[1].IsDisposed);
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var pool = new ConcurrentKeyedObjectPool<string, TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 5));

        pool.Dispose();
        pool.Dispose();
    }

    [Fact]
    public void Rent_AfterDispose_ThrowsObjectDisposedException()
    {
        var pool = new ConcurrentKeyedObjectPool<string, TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 5));
        pool.Dispose();

        Assert.Throws<ObjectDisposedException>(() => pool.Rent("a"));
    }

    [Fact]
    public async Task Rent_SameKeyFromManyThreads_KeepsOnePoolAndDisposesExtras()
    {
        var created = new System.Collections.Concurrent.ConcurrentBag<TestPool>();
        using var pool = new ConcurrentKeyedObjectPool<string, TestObject>(_ =>
        {
            var testPool = new TestPool();
            created.Add(testPool);
            return testPool;
        });
        var tasks = new List<Task>();

        for (var i = 0; i < 32; i++)
        {
            tasks.Add(Task.Run(() => pool.Rent("a"), TestContext.Current.CancellationToken));
        }

        await Task.WhenAll(tasks);

        var active = 0;

        foreach (var testPool in created)
        {
            if (!testPool.IsDisposed)
            {
                active++;
            }
        }

        Assert.Equal(1, active);
    }

    [Fact]
    public async Task RentAndReturn_ManyThreads_KeepsItemsInTheirPools()
    {
        using var pool = new ConcurrentKeyedObjectPool<string, TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 100));
        var tasks = new List<Task>();

        for (var i = 0; i < 16; i++)
        {
            var key = i % 2 == 0 ? "a" : "b";
            tasks.Add(Task.Run(() =>
            {
                for (var j = 0; j < 50; j++)
                {
                    var item = pool.Rent(key);
                    Assert.Equal(key, item.Key);
                    pool.Return(key, item);
                }
            }, TestContext.Current.CancellationToken));
        }

        await Task.WhenAll(tasks);

        Assert.True(pool.Count("a") > 0);
        Assert.True(pool.Count("b") > 0);
    }
}
