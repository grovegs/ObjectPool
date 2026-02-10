using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

using GroveGames.ObjectPool.Concurrent;

namespace GroveGames.ObjectPool.Tests.Concurrent;

public sealed class ConcurrentMultiTypeObjectPoolBuilderTests
{
    private abstract class BuilderTestBase
    {
        public string Id { get; set; } = string.Empty;
    }

    private sealed class BuilderTestDerived1 : BuilderTestBase
    {
        public int Value { get; set; }
    }

    private sealed class BuilderTestDerived2 : BuilderTestBase
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class BuilderTestDerived3 : BuilderTestBase
    {
        public bool Flag { get; set; }
    }

    [Fact]
    public void Constructor_CreatesEmptyBuilder()
    {
        // Arrange & Act
        var builder = new ConcurrentMultiTypeObjectPoolBuilder<BuilderTestBase>();
        var result = builder.Build();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void AddPool_ValidParameters_AddsPool()
    {
        // Arrange
        var builder = new ConcurrentMultiTypeObjectPoolBuilder<BuilderTestBase>();

        // Act
        var result = builder.AddPool<BuilderTestDerived1>(() => new BuilderTestDerived1(), null, null, 1, 5);

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void AddPool_WithOnRentAndOnReturn_AddsPool()
    {
        // Arrange
        var builder = new ConcurrentMultiTypeObjectPoolBuilder<BuilderTestBase>();

        // Act
        var result = builder.AddPool<BuilderTestDerived1>(
            () => new BuilderTestDerived1(),
            item => item.Value = 42,
            item => item.Value = 0,
            1, 5);

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void AddPool_DuplicateType_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ConcurrentMultiTypeObjectPoolBuilder<BuilderTestBase>();
        builder.AddPool<BuilderTestDerived1>(() => new BuilderTestDerived1(), null, null, 1, 5);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            builder.AddPool<BuilderTestDerived1>(() => new BuilderTestDerived1(), null, null, 1, 5));
    }

    [Fact]
    public void AddPool_MultipleTypes_AllAdded()
    {
        // Arrange
        var builder = new ConcurrentMultiTypeObjectPoolBuilder<BuilderTestBase>();

        // Act
        builder.AddPool<BuilderTestDerived1>(() => new BuilderTestDerived1(), null, null, 1, 5)
               .AddPool<BuilderTestDerived2>(() => new BuilderTestDerived2(), null, null, 2, 10)
               .AddPool<BuilderTestDerived3>(() => new BuilderTestDerived3(), null, null, 3, 15);

        var result = builder.Build();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(typeof(BuilderTestDerived1), result.Keys);
        Assert.Contains(typeof(BuilderTestDerived2), result.Keys);
        Assert.Contains(typeof(BuilderTestDerived3), result.Keys);
    }

    [Fact]
    public void Build_EmptyBuilder_ReturnsEmptyFrozenDictionary()
    {
        // Arrange
        var builder = new ConcurrentMultiTypeObjectPoolBuilder<BuilderTestBase>();

        // Act
        var result = builder.Build();

        // Assert
        Assert.Empty(result);
        Assert.IsAssignableFrom<System.Collections.Frozen.FrozenDictionary<Type, IObjectPool<BuilderTestBase>>>(result);
    }

    [Fact]
    public void Build_WithPools_ReturnsFrozenDictionary()
    {
        // Arrange
        var builder = new ConcurrentMultiTypeObjectPoolBuilder<BuilderTestBase>();
        builder.AddPool<BuilderTestDerived1>(() => new BuilderTestDerived1(), null, null, 1, 5);

        // Act
        var result = builder.Build();

        // Assert
        Assert.Single(result);
        Assert.Contains(typeof(BuilderTestDerived1), result.Keys);
        Assert.NotNull(result[typeof(BuilderTestDerived1)]);
    }

    [Fact]
    public void Build_CalledMultipleTimes_ReturnsSeparateInstances()
    {
        // Arrange
        var builder = new ConcurrentMultiTypeObjectPoolBuilder<BuilderTestBase>();
        builder.AddPool<BuilderTestDerived1>(() => new BuilderTestDerived1(), null, null, 1, 5);

        // Act
        var result1 = builder.Build();
        var result2 = builder.Build();

        // Assert
        Assert.NotSame(result1, result2);
        Assert.Equal(result1.Count, result2.Count);
        Assert.Equal(result1.Keys, result2.Keys);
    }

    [Fact]
    public void AddPool_AfterBuild_StillWorks()
    {
        // Arrange
        var builder = new ConcurrentMultiTypeObjectPoolBuilder<BuilderTestBase>();
        builder.AddPool<BuilderTestDerived1>(() => new BuilderTestDerived1(), null, null, 1, 5);
        var firstBuild = builder.Build();

        // Act
        builder.AddPool<BuilderTestDerived2>(() => new BuilderTestDerived2(), null, null, 2, 10);
        var secondBuild = builder.Build();

        // Assert
        Assert.Single(firstBuild);
        Assert.Equal(2, secondBuild.Count);
        Assert.Contains(typeof(BuilderTestDerived1), secondBuild.Keys);
        Assert.Contains(typeof(BuilderTestDerived2), secondBuild.Keys);
    }

    [Fact]
    public void MethodChaining_Works()
    {
        // Arrange
        var builder = new ConcurrentMultiTypeObjectPoolBuilder<BuilderTestBase>();

        // Act
        var result = builder
            .AddPool<BuilderTestDerived1>(() => new BuilderTestDerived1(), null, null, 1, 5)
            .AddPool<BuilderTestDerived2>(() => new BuilderTestDerived2(), null, null, 2, 10)
            .AddPool<BuilderTestDerived3>(() => new BuilderTestDerived3(), null, null, 3, 15)
            .Build();

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void AddPool_NullFactory_ThrowsException()
    {
        // Arrange
        var builder = new ConcurrentMultiTypeObjectPoolBuilder<BuilderTestBase>();

        // Act & Assert
        Assert.ThrowsAny<Exception>(() =>
            builder.AddPool<BuilderTestDerived1>(null!, null, null, 1, 5));
    }

    [Fact]
    public async Task AddPool_ConcurrentAccess_HandlesCorrectly()
    {
        // Arrange
        var builder = new ConcurrentMultiTypeObjectPoolBuilder<BuilderTestBase>();
        var exceptions = new ConcurrentBag<Exception>();

        // Act
        var tasks = Enumerable.Range(0, 10).Select(i => Task.Run(() =>
        {
            try
            {
                // Try to add different types concurrently
                if (i % 3 == 0)
                {
                    builder.AddPool<BuilderTestDerived1>(() => new BuilderTestDerived1(), null, null, 1, 5);
                }
                else if (i % 3 == 1)
                {
                    builder.AddPool<BuilderTestDerived2>(() => new BuilderTestDerived2(), null, null, 1, 5);
                }
                else
                {
                    builder.AddPool<BuilderTestDerived3>(() => new BuilderTestDerived3(), null, null, 1, 5);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        var result = builder.Build();
        Assert.True(result.Count <= 3);
        Assert.True(result.Count > 0);
        Assert.All(exceptions, ex => Assert.IsType<ArgumentException>(ex));
    }

    [Fact]
    public void AddPool_ZeroSizes_CreatesPool()
    {
        // Arrange
        var builder = new ConcurrentMultiTypeObjectPoolBuilder<BuilderTestBase>();

        // Act & Assert - Should not throw, assuming underlying pool handles validation
        builder.AddPool<BuilderTestDerived1>(() => new BuilderTestDerived1(), null, null, 0, 1);
        var result = builder.Build();
        Assert.Single(result);
    }

    [Fact]
    public void AddPool_LargeSizes_CreatesPool()
    {
        // Arrange
        var builder = new ConcurrentMultiTypeObjectPoolBuilder<BuilderTestBase>();

        // Act
        builder.AddPool<BuilderTestDerived1>(() => new BuilderTestDerived1(), null, null, 100, 1000);
        var result = builder.Build();

        // Assert
        Assert.Single(result);
        Assert.Contains(typeof(BuilderTestDerived1), result.Keys);
    }

    [Fact]
    public void AddPool_OnlyOnRent_CreatesPool()
    {
        // Arrange
        var builder = new ConcurrentMultiTypeObjectPoolBuilder<BuilderTestBase>();

        // Act
        builder.AddPool<BuilderTestDerived1>(
            () => new BuilderTestDerived1(),
            item => item.Value = 99,
            null,
            1, 5);
        var result = builder.Build();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public void AddPool_OnlyOnReturn_CreatesPool()
    {
        // Arrange
        var builder = new ConcurrentMultiTypeObjectPoolBuilder<BuilderTestBase>();

        // Act
        builder.AddPool<BuilderTestDerived1>(
            () => new BuilderTestDerived1(),
            null,
            item => item.Value = 0,
            1, 5);
        var result = builder.Build();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public void Build_PreservesPoolReferences()
    {
        // Arrange
        var builder = new ConcurrentMultiTypeObjectPoolBuilder<BuilderTestBase>();
        builder.AddPool<BuilderTestDerived1>(() => new BuilderTestDerived1(), null, null, 1, 5);

        // Act
        var result1 = builder.Build();
        var result2 = builder.Build();

        // Assert
        Assert.Same(result1[typeof(BuilderTestDerived1)], result2[typeof(BuilderTestDerived1)]);
    }
}
