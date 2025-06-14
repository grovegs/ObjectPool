namespace GroveGames.ObjectPool.Tests;

public sealed class ArrayPoolTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesPool()
    {
        // Arrange & Act
        using var pool = new ArrayPool<int>(5, 10);

        // Assert - pool should be created without throwing
        Assert.Equal(0, pool.Count(5));
        Assert.Equal(10, pool.MaxSize(5));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_NegativeInitialSize_ThrowsArgumentOutOfRangeException(int initialSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ArrayPool<int>(initialSize, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_NonPositiveMaxSize_ThrowsArgumentOutOfRangeException(int maxSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ArrayPool<int>(5, maxSize));
    }

    [Fact]
    public void Constructor_InitialSizeGreaterThanMaxSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ArrayPool<int>(15, 10));
    }

    [Fact]
    public void Rent_SizeZero_ReturnsEmptyArray()
    {
        // Arrange
        using var pool = new ArrayPool<int>(5, 10);

        // Act
        var array = pool.Rent(0);

        // Assert
        Assert.Empty(array);
    }

    [Fact]
    public void Rent_ValidSize_ReturnsArrayOfCorrectSize()
    {
        // Arrange
        using var pool = new ArrayPool<int>(5, 10);

        // Act
        var array = pool.Rent(5);

        // Assert
        Assert.Equal(5, array.Length);
    }

    [Fact]
    public void Rent_DifferentSizes_CreatesIndependentPools()
    {
        // Arrange
        using var pool = new ArrayPool<int>(5, 10);

        // Act
        var array5 = pool.Rent(5);
        var array10 = pool.Rent(10);

        // Assert
        Assert.Equal(5, array5.Length);
        Assert.Equal(10, array10.Length);
        Assert.Equal(0, pool.Count(5));
        Assert.Equal(0, pool.Count(10));
    }

    [Fact]
    public void Return_ValidArray_AddsToPool()
    {
        // Arrange
        using var pool = new ArrayPool<int>(5, 10);
        var array = pool.Rent(5);

        // Act
        pool.Return(array);

        // Assert
        Assert.Equal(1, pool.Count(5));
    }

    [Fact]
    public void Return_NullArray_ThrowsArgumentNullException()
    {
        // Arrange
        using var pool = new ArrayPool<int>(5, 10);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => pool.Return(null!));
    }

    [Fact]
    public void Return_WithoutExistingPool_CreatesPool()
    {
        // Arrange
        using var pool = new ArrayPool<int>(5, 10);
        var array = new int[7];

        // Act
        pool.Return(array);

        // Assert
        Assert.Equal(1, pool.Count(7));
    }

    [Fact]
    public void RentAndReturn_ReusesArrays()
    {
        // Arrange
        using var pool = new ArrayPool<int>(5, 10);
        var originalArray = pool.Rent(5);
        originalArray[0] = 42;
        pool.Return(originalArray);

        // Act
        var reusedArray = pool.Rent(5);

        // Assert
        Assert.Same(originalArray, reusedArray);
        Assert.Equal(42, reusedArray[0]);
        Assert.Equal(0, pool.Count(5));
    }

    [Fact]
    public void Count_NonExistentSize_ReturnsZero()
    {
        // Arrange
        using var pool = new ArrayPool<int>(5, 10);

        // Act
        var count = pool.Count(100);

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public void Count_ExistingPool_ReturnsCorrectCount()
    {
        // Arrange
        using var pool = new ArrayPool<int>(5, 10);
        pool.Return(new int[5]);
        pool.Return(new int[5]);
        pool.Return(new int[5]);

        // Act
        var count = pool.Count(5);

        // Assert
        Assert.Equal(3, count);
    }

    [Fact]
    public void MaxSize_PositiveSize_ReturnsConfiguredMaxSize()
    {
        // Arrange
        using var pool = new ArrayPool<int>(5, 20);

        // Act
        var maxSize = pool.MaxSize(10);

        // Assert
        Assert.Equal(20, maxSize);
    }

    [Fact]
    public void MaxSize_ExistingPool_ReturnsConfiguredMaxSize()
    {
        // Arrange
        using var pool = new ArrayPool<int>(5, 15);
        pool.Rent(7);

        // Act
        var maxSize = pool.MaxSize(7);

        // Assert
        Assert.Equal(15, maxSize);
    }

    [Fact]
    public void Clear_RemovesAllArraysFromAllPools()
    {
        // Arrange
        using var pool = new ArrayPool<int>(5, 10);
        pool.Return(new int[5]);
        pool.Return(new int[10]);
        pool.Return(new int[15]);

        // Act
        pool.Clear();

        // Assert
        Assert.Equal(0, pool.Count(5));
        Assert.Equal(0, pool.Count(10));
        Assert.Equal(0, pool.Count(15));
    }

    [Fact]
    public void Rent_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ArrayPool<int>(5, 10);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Rent(5));
    }

    [Fact]
    public void Return_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ArrayPool<int>(5, 10);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Return(new int[5]));
    }

    [Fact]
    public void Count_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ArrayPool<int>(5, 10);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Count(5));
    }

    [Fact]
    public void MaxSize_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ArrayPool<int>(5, 10);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.MaxSize(5));
    }

    [Fact]
    public void Clear_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new ArrayPool<int>(5, 10);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Clear());
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        // Arrange
        var pool = new ArrayPool<int>(5, 10);

        // Act & Assert
        pool.Dispose();
        pool.Dispose(); // Should not throw
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 10)]
    [InlineData(0, 100)]
    public void Constructor_ValidSizeCombinations_CreatesPool(int initialSize, int maxSize)
    {
        // Arrange & Act
        using var pool = new ArrayPool<int>(initialSize, maxSize);

        // Assert
        Assert.Equal(0, pool.Count(5));
        Assert.Equal(maxSize, pool.MaxSize(5));
    }

    [Fact]
    public void Return_MultipleArraysSameSize_RespectMaxSize()
    {
        // Arrange
        using var pool = new ArrayPool<int>(0, 3);

        // Act
        pool.Return(new int[5]);
        pool.Return(new int[5]);
        pool.Return(new int[5]);
        pool.Return(new int[5]);

        // Assert
        Assert.True(pool.Count(5) <= 3);
    }

    [Fact]
    public void Rent_MultipleSizes_CreatesIndependentPools()
    {
        // Arrange
        using var pool = new ArrayPool<string>(2, 5);

        // Act
        var array1 = pool.Rent(1);
        var array3 = pool.Rent(3);
        var array5 = pool.Rent(5);
        pool.Return(new string[1]);
        pool.Return(new string[3]);

        // Assert
        Assert.Single(array1);
        Assert.Equal(3, array3.Length);
        Assert.Equal(5, array5.Length);
        Assert.Equal(1, pool.Count(1));
        Assert.Equal(1, pool.Count(3));
        Assert.Equal(0, pool.Count(5));
    }

    [Fact]
    public void MaxSize_ZeroSize_ReturnsZero()
    {
        // Arrange
        using var pool = new ArrayPool<int>(5, 10);

        // Act
        var maxSize = pool.MaxSize(0);

        // Assert
        Assert.Equal(0, maxSize);
    }

    [Fact]
    public void Clear_EmptyPool_DoesNotThrow()
    {
        // Arrange
        using var pool = new ArrayPool<int>(5, 10);

        // Act & Assert
        pool.Clear();
        Assert.Equal(0, pool.Count(5));
    }
}