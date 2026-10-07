using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GroveGames.ObjectPool.Concurrent;

namespace GroveGames.ObjectPool.Tests.Concurrent;

public sealed class ConcurrentIndexedObjectPoolTests
{
    private sealed class TestObject
    {
        public int Key { get; set; }
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
        Assert.Throws<ArgumentNullException>(() => new ConcurrentIndexedObjectPool<TestObject>(null!));
    }

    [Fact]
    public void Count_UnusedKey_ReturnsZero()
    {
        using var pool = new ConcurrentIndexedObjectPool<TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 5, 10));

        Assert.Equal(0, pool.Count(0));
    }

    [Fact]
    public void MaxSize_UnusedKey_ReturnsZero()
    {
        using var pool = new ConcurrentIndexedObjectPool<TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 5, 10));

        Assert.Equal(0, pool.MaxSize(0));
    }

    [Fact]
    public void MaxSize_UsedKey_ReturnsPoolMaxSize()
    {
        using var pool = new ConcurrentIndexedObjectPool<TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 15));

        pool.Rent(0);

        Assert.Equal(15, pool.MaxSize(0));
    }

    [Fact]
    public void Rent_NewKey_ReturnsItemFromKeyPool()
    {
        using var pool = new ConcurrentIndexedObjectPool<TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 5));

        var item = pool.Rent(1);

        Assert.Equal(1, item.Key);
    }

    [Fact]
    public void Rent_SameKeyTwice_InvokesFactoryOnce()
    {
        var factoryCalls = 0;
        using var pool = new ConcurrentIndexedObjectPool<TestObject>(key =>
        {
            factoryCalls++;
            return new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 5);
        });

        pool.Rent(0);
        pool.Rent(0);

        Assert.Equal(1, factoryCalls);
    }

    [Fact]
    public void Rent_DifferentKeys_UsesSeparatePools()
    {
        using var pool = new ConcurrentIndexedObjectPool<TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 5));

        var first = pool.Rent(0);
        var second = pool.Rent(1);
        pool.Return(0, first);

        Assert.Equal(0, first.Key);
        Assert.Equal(1, second.Key);
        Assert.Equal(1, pool.Count(0));
        Assert.Equal(0, pool.Count(1));
    }

    [Fact]
    public void Rent_FactoryReturnsNull_ThrowsArgumentNullException()
    {
        using var pool = new ConcurrentIndexedObjectPool<TestObject>(_ => null!);

        Assert.Throws<ArgumentNullException>(() => pool.Rent(0));
    }

    [Fact]
    public void Rent_WithOnRentCallback_InvokesCallback()
    {
        using var pool = new ConcurrentIndexedObjectPool<TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, static item => item.IsRented = true, null, 0, 5));

        var item = pool.Rent(0);

        Assert.True(item.IsRented);
    }

    [Fact]
    public void Return_ItemToPool_AddsToPool()
    {
        using var pool = new ConcurrentIndexedObjectPool<TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 5));
        var item = pool.Rent(0);

        pool.Return(0, item);

        Assert.Equal(1, pool.Count(0));
    }

    [Fact]
    public void Return_WithOnReturnCallback_InvokesCallback()
    {
        using var pool = new ConcurrentIndexedObjectPool<TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, static item => item.IsReturned = true, 0, 5));
        var item = pool.Rent(0);

        pool.Return(0, item);

        Assert.True(item.IsReturned);
    }

    [Fact]
    public void Return_UnusedKey_CreatesPoolAndStoresItem()
    {
        using var pool = new ConcurrentIndexedObjectPool<TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 5));

        pool.Return(2, new TestObject { Key = 2 });

        Assert.Equal(1, pool.Count(2));
    }

    [Fact]
    public void Warm_Key_PreAllocatesItems()
    {
        using var pool = new ConcurrentIndexedObjectPool<TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 5, 10));

        pool.Warm(0);

        Assert.Equal(5, pool.Count(0));
        Assert.Equal(0, pool.Count(1));
    }

    [Fact]
    public void Warm_NoParameters_WarmsCreatedPools()
    {
        using var pool = new ConcurrentIndexedObjectPool<TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 5, 10));
        pool.Rent(0);
        pool.Rent(1);

        pool.Warm();

        Assert.Equal(5, pool.Count(0));
        Assert.Equal(5, pool.Count(1));
        Assert.Equal(0, pool.Count(2));
    }

    [Fact]
    public void Clear_Key_ClearsOnlyThatPool()
    {
        using var pool = new ConcurrentIndexedObjectPool<TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 5, 10));
        pool.Warm(0);
        pool.Warm(1);

        pool.Clear(0);

        Assert.Equal(0, pool.Count(0));
        Assert.Equal(5, pool.Count(1));
    }

    [Fact]
    public void Clear_UnusedKey_DoesNotCreatePool()
    {
        var factoryCalls = 0;
        using var pool = new ConcurrentIndexedObjectPool<TestObject>(key =>
        {
            factoryCalls++;
            return new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 5);
        });

        pool.Clear(0);

        Assert.Equal(0, factoryCalls);
    }

    [Fact]
    public void Clear_NoParameters_ClearsAllPools()
    {
        using var pool = new ConcurrentIndexedObjectPool<TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 5, 10));
        pool.Warm(0);
        pool.Warm(1);

        pool.Clear();

        Assert.Equal(0, pool.Count(0));
        Assert.Equal(0, pool.Count(1));
    }

    [Fact]
    public void Dispose_CreatedPools_DisposesEachPool()
    {
        var created = new TestPool[2];
        var index = 0;
        var pool = new ConcurrentIndexedObjectPool<TestObject>(_ => created[index++] = new TestPool());
        pool.Rent(0);
        pool.Rent(1);

        pool.Dispose();

        Assert.True(created[0].IsDisposed);
        Assert.True(created[1].IsDisposed);
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var pool = new ConcurrentIndexedObjectPool<TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 5));

        pool.Dispose();
        pool.Dispose();
    }

    [Fact]
    public void Rent_AfterDispose_ThrowsObjectDisposedException()
    {
        var pool = new ConcurrentIndexedObjectPool<TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 5));
        pool.Dispose();

        Assert.Throws<ObjectDisposedException>(() => pool.Rent(0));
    }

    [Fact]
    public void Rent_NegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        using var pool = new ConcurrentIndexedObjectPool<TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 5));

        Assert.Throws<ArgumentOutOfRangeException>(() => pool.Rent(-1));
    }

    [Fact]
    public void Count_NegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        using var pool = new ConcurrentIndexedObjectPool<TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 5));

        Assert.Throws<ArgumentOutOfRangeException>(() => pool.Count(-1));
    }

    [Fact]
    public void Rent_IndexBeyondCapacity_KeepsEarlierPools()
    {
        using var pool = new ConcurrentIndexedObjectPool<TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 5));
        var first = pool.Rent(1);
        pool.Return(1, first);

        var far = pool.Rent(64);

        Assert.Equal(64, far.Key);
        Assert.Equal(1, pool.Count(1));
        Assert.Same(first, pool.Rent(1));
    }

    [Fact]
    public void Rent_LargeIndex_CreatesPool()
    {
        using var pool = new ConcurrentIndexedObjectPool<TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 5));

        var item = pool.Rent(1000);

        Assert.Equal(1000, item.Key);
    }

    [Fact]
    public async Task Rent_SameKeyFromManyThreads_KeepsOnePoolAndDisposesExtras()
    {
        var created = new System.Collections.Concurrent.ConcurrentBag<TestPool>();
        using var pool = new ConcurrentIndexedObjectPool<TestObject>(_ =>
        {
            var testPool = new TestPool();
            created.Add(testPool);
            return testPool;
        });
        var tasks = new List<Task>();

        for (var i = 0; i < 32; i++)
        {
            tasks.Add(Task.Run(() => pool.Rent(0), TestContext.Current.CancellationToken));
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
        using var pool = new ConcurrentIndexedObjectPool<TestObject>(key => new ConcurrentObjectPool<TestObject>(() => new TestObject { Key = key }, null, null, 0, 100));
        var tasks = new List<Task>();

        for (var i = 0; i < 16; i++)
        {
            var key = i % 2 == 0 ? 0 : 1;
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

        Assert.True(pool.Count(0) > 0);
        Assert.True(pool.Count(1) > 0);
    }
}
