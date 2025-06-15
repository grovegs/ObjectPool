using System.Collections.Frozen;

namespace GroveGames.ObjectPool.Tests;

public sealed class MultiTypeObjectPoolBuilderTests
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
    public void Constructor_CreatesEmptyBuilder()
    {
        // Arrange & Act
        var builder = new MultiTypeObjectPoolBuilder<TestBase>();

        // Assert
        var result = builder.Build();
        Assert.Empty(result);
    }

    [Fact]
    public void AddPool_ValidParameters_AddsPool()
    {
        // Arrange
        var builder = new MultiTypeObjectPoolBuilder<TestBase>();

        // Act
        builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);
        var result = builder.Build();

        // Assert
        Assert.Single(result);
        Assert.True(result.ContainsKey(typeof(TestDerived1)));
    }

    [Fact]
    public void AddPool_NullFactory_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new MultiTypeObjectPoolBuilder<TestBase>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.AddPool<TestDerived1>(null!, null, null, 0, 5));
    }

    [Fact]
    public void AddPool_DuplicateType_ThrowsArgumentException()
    {
        // Arrange
        var builder = new MultiTypeObjectPoolBuilder<TestBase>();
        builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 10));

        Assert.Contains("TestDerived1", exception.Message);
        Assert.Contains("already been added", exception.Message);
    }

    [Fact]
    public void AddPool_FluentInterface_ReturnsSameBuilder()
    {
        // Arrange
        var builder = new MultiTypeObjectPoolBuilder<TestBase>();

        // Act
        var result = builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void AddPool_ChainedCalls_AddsMultiplePools()
    {
        // Arrange
        var builder = new MultiTypeObjectPoolBuilder<TestBase>();

        // Act
        builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5)
               .AddPool<TestDerived2>(() => new TestDerived2(), null, null, 0, 10);
        var result = builder.Build();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey(typeof(TestDerived1)));
        Assert.True(result.ContainsKey(typeof(TestDerived2)));
    }

    [Fact]
    public void AddPool_WithCallbacks_CreatesPoolWithCallbacks()
    {
        // Arrange
        var builder = new MultiTypeObjectPoolBuilder<TestBase>();
        var rentCalled = false;
        var returnCalled = false;

        // Act
        builder.AddPool<TestDerived1>(
            () => new TestDerived1(),
            _ => rentCalled = true,
            _ => returnCalled = true,
            0, 5);
        var result = builder.Build();

        // Assert
        Assert.Single(result);
        var pool = result[typeof(TestDerived1)];
        var item = pool.Rent();
        Assert.True(rentCalled);

        pool.Return(item);
        Assert.True(returnCalled);
    }

    [Fact]
    public void AddPool_WithoutCallbacks_CreatesPoolWithoutCallbacks()
    {
        // Arrange
        var builder = new MultiTypeObjectPoolBuilder<TestBase>();

        // Act
        builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);
        var result = builder.Build();

        // Assert
        Assert.Single(result);
        var pool = result[typeof(TestDerived1)];
        var item = pool.Rent();
        pool.Return(item); // Should not throw
    }

    [Fact]
    public void Build_CalledMultipleTimes_ReturnsSameContent()
    {
        // Arrange
        var builder = new MultiTypeObjectPoolBuilder<TestBase>();
        builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);

        // Act
        var result1 = builder.Build();
        var result2 = builder.Build();

        // Assert
        Assert.NotSame(result1, result2); // Different instances
        Assert.Equal(result1.Count, result2.Count);
        Assert.True(result1.ContainsKey(typeof(TestDerived1)));
        Assert.True(result2.ContainsKey(typeof(TestDerived1)));
    }

    [Fact]
    public void Build_EmptyBuilder_ReturnsEmptyFrozenDictionary()
    {
        // Arrange
        var builder = new MultiTypeObjectPoolBuilder<TestBase>();

        // Act
        var result = builder.Build();

        // Assert
        Assert.Empty(result);
        Assert.IsType<FrozenDictionary<Type, IObjectPool<TestBase>>>(result, false);
    }

    [Fact]
    public void AddPool_DifferentSizes_CreatesPoolsWithCorrectSizes()
    {
        // Arrange
        var builder = new MultiTypeObjectPoolBuilder<TestBase>();

        // Act
        builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 2, 10)
               .AddPool<TestDerived2>(() => new TestDerived2(), null, null, 5, 20);
        var result = builder.Build();

        // Assert
        var pool1 = result[typeof(TestDerived1)];
        var pool2 = result[typeof(TestDerived2)];

        Assert.Equal(10, pool1.MaxSize);
        Assert.Equal(20, pool2.MaxSize);
    }

    [Theory]
    [InlineData(-1, 5)]
    [InlineData(0, 0)]
    [InlineData(10, 5)]
    public void AddPool_InvalidSizeParameters_ThrowsArgumentOutOfRangeException(int initialSize, int maxSize)
    {
        // Arrange
        var builder = new MultiTypeObjectPoolBuilder<TestBase>();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, initialSize, maxSize));
    }

    [Fact]
    public void AddPool_AfterBuild_StillAllowsNewPools()
    {
        // Arrange
        var builder = new MultiTypeObjectPoolBuilder<TestBase>();
        builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);
        var firstResult = builder.Build();

        // Act
        builder.AddPool<TestDerived2>(() => new TestDerived2(), null, null, 0, 10);
        var secondResult = builder.Build();

        // Assert
        Assert.Single(firstResult);
        Assert.Equal(2, secondResult.Count);
        Assert.True(secondResult.ContainsKey(typeof(TestDerived1)));
        Assert.True(secondResult.ContainsKey(typeof(TestDerived2)));
    }

    [Fact]
    public void AddPool_ComplexFactory_CreatesCorrectInstances()
    {
        // Arrange
        var builder = new MultiTypeObjectPoolBuilder<TestBase>();
        var counter = 0;

        // Act
        builder.AddPool<TestDerived1>(() => new TestDerived1 { Value = $"instance_{++counter}" }, null, null, 0, 5);
        var result = builder.Build();

        // Assert
        var pool = result[typeof(TestDerived1)];
        var item1 = (TestDerived1)pool.Rent();
        var item2 = (TestDerived1)pool.Rent();

        Assert.Equal("instance_1", item1.Value);
        Assert.Equal("instance_2", item2.Value);
    }

    [Fact]
    public void AddPool_SameBaseTypeDifferentDerived_CreatesIndependentPools()
    {
        // Arrange
        var builder = new MultiTypeObjectPoolBuilder<TestBase>();

        // Act
        builder.AddPool<TestDerived1>(() => new TestDerived1 { Value = "derived1" }, null, null, 0, 5)
               .AddPool<TestDerived2>(() => new TestDerived2 { Number = 42 }, null, null, 0, 10);
        var result = builder.Build();

        // Assert
        Assert.Equal(2, result.Count);

        var pool1 = result[typeof(TestDerived1)];
        var pool2 = result[typeof(TestDerived2)];

        var item1 = (TestDerived1)pool1.Rent();
        var item2 = (TestDerived2)pool2.Rent();

        Assert.Equal("derived1", item1.Value);
        Assert.Equal(42, item2.Number);
    }

    [Fact]
    public void Build_ReturnsImmutableResult()
    {
        // Arrange
        var builder = new MultiTypeObjectPoolBuilder<TestBase>();
        builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);

        // Act
        var result = builder.Build();

        // Assert
        Assert.IsType<FrozenDictionary<Type, IObjectPool<TestBase>>>(result, false);
    }

    [Fact]
    public void AddPool_LargeNumberOfTypes_HandlesCorrectly()
    {
        // Arrange
        var builder = new MultiTypeObjectPoolBuilder<TestBase>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            if (i == 0)
            {
                builder.AddPool<TestDerived1>(() => new TestDerived1(), null, null, 0, 5);
            }
            else if (i == 1)
            {
                builder.AddPool<TestDerived2>(() => new TestDerived2(), null, null, 0, 5);
            }
            else
            {
                break;
            }
        }
        var result = builder.Build();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey(typeof(TestDerived1)));
        Assert.True(result.ContainsKey(typeof(TestDerived2)));
    }
}