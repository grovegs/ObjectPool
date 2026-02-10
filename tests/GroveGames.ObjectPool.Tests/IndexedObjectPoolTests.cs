using System;

namespace GroveGames.ObjectPool.Tests;

public sealed class IndexedObjectPoolTests
{
    private sealed class TestObject
    {
        public int Key { get; set; }
        public bool IsRented { get; set; }
        public bool IsReturned { get; set; }
    }

    [Fact]
    public void Constructor_ValidParameters_CreatesPool()
    {
        using var pool = new IndexedObjectPool<TestObject>(3, key => new TestObject { Key = key }, null, null, 5, 10);

        Assert.Equal(0, pool.Count(0));
        Assert.Equal(10, pool.MaxSize(0));
        Assert.Equal(0, pool.Count(1));
        Assert.Equal(10, pool.MaxSize(1));
        Assert.Equal(0, pool.Count(2));
        Assert.Equal(10, pool.MaxSize(2));
    }

    [Fact]
    public void Constructor_NullFactory_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new IndexedObjectPool<TestObject>(3, null!, null, null, 5, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_NonPositivePoolCount_ThrowsArgumentOutOfRangeException(int poolCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new IndexedObjectPool<TestObject>(poolCount, key => new TestObject { Key = key }, null, null, 5, 10));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_NegativeInitialSize_ThrowsArgumentOutOfRangeException(int initialSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new IndexedObjectPool<TestObject>(3, key => new TestObject { Key = key }, null, null, initialSize, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_NonPositiveMaxSize_ThrowsArgumentOutOfRangeException(int maxSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new IndexedObjectPool<TestObject>(3, key => new TestObject { Key = key }, null, null, 5, maxSize));
    }

    [Fact]
    public void Constructor_InitialSizeGreaterThanMaxSize_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new IndexedObjectPool<TestObject>(3, key => new TestObject { Key = key }, null, null, 15, 10));
    }

    [Fact]
    public void Rent_ValidKey_ReturnsObjectWithCorrectKey()
    {
        using var pool = new IndexedObjectPool<TestObject>(3, key => new TestObject { Key = key }, null, null, 0, 5);

        var item0 = pool.Rent(0);
        var item1 = pool.Rent(1);
        var item2 = pool.Rent(2);

        Assert.Equal(0, item0.Key);
        Assert.Equal(1, item1.Key);
        Assert.Equal(2, item2.Key);
    }

    [Fact]
    public void Rent_WithOnRentCallback_InvokesCallback()
    {
        int rentCount = 0;
        using var pool = new IndexedObjectPool<TestObject>(
            3,
            key => new TestObject { Key = key },
            obj => { obj.IsRented = true; rentCount++; },
            null,
            0,
            5);

        var item = pool.Rent(1);

        Assert.True(item.IsRented);
        Assert.Equal(1, rentCount);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    [InlineData(10)]
    public void Rent_InvalidKey_ThrowsArgumentOutOfRangeException(int key)
    {
        using var pool = new IndexedObjectPool<TestObject>(3, k => new TestObject { Key = k }, null, null, 0, 5);

        Assert.Throws<ArgumentOutOfRangeException>(() => pool.Rent(key));
    }

    [Fact]
    public void Return_ItemToPool_AddsToPool()
    {
        using var pool = new IndexedObjectPool<TestObject>(3, key => new TestObject { Key = key }, null, null, 0, 5);
        var item = pool.Rent(1);

        pool.Return(1, item);

        Assert.Equal(1, pool.Count(1));
    }

    [Fact]
    public void Return_WithOnReturnCallback_InvokesCallback()
    {
        int returnCount = 0;
        using var pool = new IndexedObjectPool<TestObject>(
            3,
            key => new TestObject { Key = key },
            null,
            obj => { obj.IsReturned = true; returnCount++; },
            0,
            5);
        var item = pool.Rent(1);

        pool.Return(1, item);

        Assert.True(item.IsReturned);
        Assert.Equal(1, returnCount);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void Return_InvalidKey_DoesNotThrow(int key)
    {
        using var pool = new IndexedObjectPool<TestObject>(3, k => new TestObject { Key = k }, null, null, 0, 5);
        var item = new TestObject { Key = key };

        pool.Return(key, item);

        Assert.Equal(0, pool.Count(0));
    }

    [Fact]
    public void Count_ValidKey_ReturnsCorrectCount()
    {
        using var pool = new IndexedObjectPool<TestObject>(3, key => new TestObject { Key = key }, null, null, 0, 5);

        pool.Return(1, new TestObject { Key = 1 });
        pool.Return(1, new TestObject { Key = 1 });

        Assert.Equal(2, pool.Count(1));
        Assert.Equal(0, pool.Count(0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void Count_InvalidKey_ReturnsZero(int key)
    {
        using var pool = new IndexedObjectPool<TestObject>(3, k => new TestObject { Key = k }, null, null, 0, 5);

        Assert.Equal(0, pool.Count(key));
    }

    [Fact]
    public void MaxSize_ValidKey_ReturnsCorrectMaxSize()
    {
        using var pool = new IndexedObjectPool<TestObject>(3, key => new TestObject { Key = key }, null, null, 0, 15);

        Assert.Equal(15, pool.MaxSize(0));
        Assert.Equal(15, pool.MaxSize(1));
        Assert.Equal(15, pool.MaxSize(2));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void MaxSize_InvalidKey_ReturnsZero(int key)
    {
        using var pool = new IndexedObjectPool<TestObject>(3, k => new TestObject { Key = k }, null, null, 0, 5);

        Assert.Equal(0, pool.MaxSize(key));
    }

    [Fact]
    public void Warm_ValidKey_PreAllocatesItems()
    {
        using var pool = new IndexedObjectPool<TestObject>(3, key => new TestObject { Key = key }, null, null, 5, 10);

        pool.Warm(1);

        Assert.Equal(5, pool.Count(1));
        Assert.Equal(0, pool.Count(0));
        Assert.Equal(0, pool.Count(2));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void Warm_InvalidKey_ThrowsArgumentOutOfRangeException(int key)
    {
        using var pool = new IndexedObjectPool<TestObject>(3, k => new TestObject { Key = k }, null, null, 5, 10);

        Assert.Throws<ArgumentOutOfRangeException>(() => pool.Warm(key));
    }

    [Fact]
    public void Warm_NoParameters_PreAllocatesAllPools()
    {
        using var pool = new IndexedObjectPool<TestObject>(3, key => new TestObject { Key = key }, null, null, 5, 10);

        pool.Warm();

        Assert.Equal(5, pool.Count(0));
        Assert.Equal(5, pool.Count(1));
        Assert.Equal(5, pool.Count(2));
    }

    [Fact]
    public void Clear_ValidKey_ClearsSpecificPool()
    {
        using var pool = new IndexedObjectPool<TestObject>(3, key => new TestObject { Key = key }, null, null, 5, 10);
        pool.Warm();

        pool.Clear(1);

        Assert.Equal(5, pool.Count(0));
        Assert.Equal(0, pool.Count(1));
        Assert.Equal(5, pool.Count(2));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void Clear_InvalidKey_DoesNotThrow(int key)
    {
        using var pool = new IndexedObjectPool<TestObject>(3, k => new TestObject { Key = k }, null, null, 5, 10);
        pool.Warm();

        pool.Clear(key);

        Assert.Equal(5, pool.Count(0));
    }

    [Fact]
    public void Clear_NoParameters_ClearsAllPools()
    {
        using var pool = new IndexedObjectPool<TestObject>(3, key => new TestObject { Key = key }, null, null, 5, 10);
        pool.Warm();

        pool.Clear();

        Assert.Equal(0, pool.Count(0));
        Assert.Equal(0, pool.Count(1));
        Assert.Equal(0, pool.Count(2));
    }

    [Fact]
    public void Dispose_DisposesPool()
    {
        var pool = new IndexedObjectPool<TestObject>(3, key => new TestObject { Key = key }, null, null, 0, 5);

        pool.Dispose();

        Assert.Throws<ObjectDisposedException>(() => pool.Rent(0));
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var pool = new IndexedObjectPool<TestObject>(3, key => new TestObject { Key = key }, null, null, 0, 5);

        pool.Dispose();
        pool.Dispose();
    }
}
