namespace GroveGames.ObjectPool.Tests;

public sealed class MultiTypeObjectPoolTests
{
    private abstract class TestBase
    {
    }

    private sealed class TestDerived1 : TestBase
    {
        public string Value { get; set; } = string.Empty;
    }

    private sealed class TestDerived2 : TestBase
    {
        public int Number { get; set; }
    }

    [Fact]
    public void Constructor_ValidConfiguration_CreatesPool()
    {
        // Arrange & Act
        using var pool = new MultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);
            builder.AddPool<TestDerived2>(() => new TestDerived2(), null, null, 0, 10);
        });

        // Assert
        Assert.Equal(0, pool.Count<TestDerived1>());
        Assert.Equal(5, pool.MaxSize<TestDerived1>());
        Assert.Equal(0, pool.Count<TestDerived2>());
        Assert.Equal(10, pool.MaxSize<TestDerived2>());
    }

    [Fact]
    public void Constructor_NullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MultiTypeObjectPool<TestBase>(null!));
    }

    [Fact]
    public void Rent_RegisteredType_ReturnsInstance()
    {
        // Arrange
        using var pool = new MultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1 { Value = "test" }, null, null, 0, 5);
        });

        // Act
        var item = pool.Rent<TestDerived1>();

        // Assert
        Assert.IsType<TestDerived1>(item);
        Assert.Equal("test", ((TestDerived1)item).Value);
    }

    [Fact]
    public void Rent_UnregisteredType_ThrowsInvalidOperationException()
    {
        // Arrange
        using var pool = new MultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);
        });

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(pool.Rent<TestDerived2>);
        Assert.Contains("TestDerived2", exception.Message);
        Assert.Contains("not registered", exception.Message);
    }

    [Fact]
    public void Return_RegisteredType_AddsToPool()
    {
        // Arrange
        using var pool = new MultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);
        });
        var item = pool.Rent<TestDerived1>();

        // Act
        pool.Return((TestDerived1)item);

        // Assert
        Assert.Equal(1, pool.Count<TestDerived1>());
    }

    [Fact]
    public void Return_UnregisteredType_DoesNotThrow()
    {
        // Arrange
        using var pool = new MultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);
        });
        var item = new TestDerived2();

        // Act & Assert
        pool.Return(item);
    }

    [Fact]
    public void RentAndReturn_SameType_ReusesInstance()
    {
        // Arrange
        using var pool = new MultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);
        });
        var originalItem = (TestDerived1)pool.Rent<TestDerived1>();
        pool.Return(originalItem);

        // Act
        var reusedItem = (TestDerived1)pool.Rent<TestDerived1>();

        // Assert
        Assert.Same(originalItem, reusedItem);
        Assert.Equal(0, pool.Count<TestDerived1>());
    }

    [Fact]
    public void Count_UnregisteredType_ReturnsZero()
    {
        // Arrange
        using var pool = new MultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);
        });

        // Act
        var count = pool.Count<TestDerived2>();

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public void MaxSize_UnregisteredType_ReturnsZero()
    {
        // Arrange
        using var pool = new MultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);
        });

        // Act
        var maxSize = pool.MaxSize<TestDerived2>();

        // Assert
        Assert.Equal(0, maxSize);
    }

    [Fact]
    public void AddPool_DuplicateType_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            new MultiTypeObjectPool<TestBase>(builder =>
            {
                builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);
                builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 10); // Duplicate
            });
        });

        Assert.Contains("TestDerived1", exception.Message);
        Assert.Contains("already been added", exception.Message);
    }

    [Fact]
    public void AddPool_FluentInterface_AllowsChaining()
    {
        // Arrange & Act
        using var pool = new MultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5)
                   .AddPool<TestDerived2>(() => new TestDerived2(), null, null, 0, 10);
        });

        // Assert
        Assert.Equal(0, pool.Count<TestDerived1>());
        Assert.Equal(5, pool.MaxSize<TestDerived1>());
        Assert.Equal(0, pool.Count<TestDerived2>());
        Assert.Equal(10, pool.MaxSize<TestDerived2>());
    }

    [Fact]
    public void Clear_RemovesAllItemsFromAllPools()
    {
        // Arrange
        using var pool = new MultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);
            builder.AddPool<TestDerived2>(() => new TestDerived2(), null, null, 0, 5);
        });

        var item1 = (TestDerived1)pool.Rent<TestDerived1>();
        var item2 = (TestDerived2)pool.Rent<TestDerived2>();
        pool.Return(item1);
        pool.Return(item2);

        // Act
        pool.Clear();

        // Assert
        Assert.Equal(0, pool.Count<TestDerived1>());
        Assert.Equal(0, pool.Count<TestDerived2>());
    }

    [Fact]
    public void Count_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new MultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);
        });
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Count<TestDerived1>());
    }

    [Fact]
    public void MaxSize_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new MultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);
        });
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.MaxSize<TestDerived1>());
    }

    [Fact]
    public void Rent_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new MultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);
        });
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(pool.Rent<TestDerived1>);
    }

    [Fact]
    public void Return_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new MultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);
        });
        var item = new TestDerived1();
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Return(item));
    }

    [Fact]
    public void Clear_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new MultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);
        });
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(pool.Clear);
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        // Arrange
        var pool = new MultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);
        });

        // Act & Assert
        pool.Dispose();
        pool.Dispose();
    }

    [Fact]
    public void Pool_MultipleTypes_IndependentPooling()
    {
        // Arrange
        using var pool = new MultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);
            builder.AddPool<TestDerived2>(() => new TestDerived2(), null, null, 0, 10);
        });

        // Act
        var item1a = (TestDerived1)pool.Rent<TestDerived1>();
        var item1b = (TestDerived1)pool.Rent<TestDerived1>();
        var item2a = (TestDerived2)pool.Rent<TestDerived2>();
        var item2b = (TestDerived2)pool.Rent<TestDerived2>();

        pool.Return(item1a);
        pool.Return(item2a);

        // Assert
        Assert.Equal(1, pool.Count<TestDerived1>());
        Assert.Equal(1, pool.Count<TestDerived2>());
        Assert.Equal(5, pool.MaxSize<TestDerived1>());
        Assert.Equal(10, pool.MaxSize<TestDerived2>());
    }

    [Fact]
    public void Pool_WithOnRentAndOnReturnCallbacks_InvokesCallbacks()
    {
        // Arrange
        var rentCount = 0;
        var returnCount = 0;

        using var pool = new MultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(
                factory: () => new TestDerived1(),
                onRent: _ => rentCount++,
                onReturn: _ => returnCount++,
                initialSize: 0,
                maxSize: 5);
        });

        // Act
        var item = (TestDerived1)pool.Rent<TestDerived1>();
        pool.Return(item);

        // Assert
        Assert.Equal(1, rentCount);
        Assert.Equal(1, returnCount);
    }

    [Fact]
    public void Pool_EmptyConfiguration_CreatesEmptyPool()
    {
        // Arrange & Act
        using var pool = new MultiTypeObjectPool<TestBase>(_ => { });

        // Assert
        Assert.Equal(0, pool.Count<TestDerived1>());
        Assert.Equal(0, pool.MaxSize<TestDerived1>());
    }

    [Fact]
    public void Return_WrongTypeInstance_DoesNotAffectCorrectTypePool()
    {
        // Arrange
        using var pool = new MultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);
        });

        var correctItem = (TestDerived1)pool.Rent<TestDerived1>();
        var wrongItem = new TestDerived2();
        pool.Return(correctItem);

        // Act
        pool.Return(wrongItem);

        // Assert
        Assert.Equal(1, pool.Count<TestDerived1>());
    }

    [Fact]
    public void Clear_EmptyPools_DoesNotThrow()
    {
        // Arrange
        using var pool = new MultiTypeObjectPool<TestBase>(builder =>
        {
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);
            builder.AddPool<TestDerived2>(() => new TestDerived2(), null, null, 0, 5);
        });

        // Act & Assert
        pool.Clear();
        Assert.Equal(0, pool.Count<TestDerived1>());
        Assert.Equal(0, pool.Count<TestDerived2>());
    }
}
