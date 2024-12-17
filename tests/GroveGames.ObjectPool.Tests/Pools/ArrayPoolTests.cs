using GroveGames.ObjectPool.Pools;

namespace GroveGames.ObjectPool.Tests.Pools;

public class ArrayPoolTests
{
    [Fact]
    public void Get_ShouldReturnArray_WithDefaultSize()
    {
        // Arrange
        int defaultSize = 128;
        var arrayPool = new ArrayPool<int>(defaultSize);

        // Act
        var array = arrayPool.Get();

        // Assert
        Assert.NotNull(array);
        Assert.Equal(defaultSize, array.Length);
    }

    [Fact]
    public void Get_ShouldResizeArray_WhenRequestedSizeIsLarger()
    {
        // Arrange
        int defaultSize = 128;
        int requestedSize = 256;
        var arrayPool = new ArrayPool<int>(defaultSize);

        // Act
        var array = arrayPool.Get(requestedSize);

        // Assert
        Assert.NotNull(array);
        Assert.True(array.Length >= requestedSize);
    }

    [Fact]
    public void Get_ShouldReuseArray_WhenReturnedToPool()
    {
        // Arrange
        int defaultSize = 128;
        var arrayPool = new ArrayPool<int>(defaultSize);

        // Act
        var array1 = arrayPool.Get();
        arrayPool.Return(array1);
        var array2 = arrayPool.Get();

        // Assert
        Assert.Same(array1, array2);
    }

    [Fact]
    public void Return_ShouldNotBreak_WhenArrayIsResized()
    {
        // Arrange
        int defaultSize = 128;
        int oversizedLength = 256;
        var arrayPool = new ArrayPool<int>(defaultSize);

        // Act
        var oversizedArray = arrayPool.Get(oversizedLength);
        arrayPool.Return(oversizedArray); // Should not throw

        // Assert
        Assert.NotNull(oversizedArray);
        Assert.Equal(oversizedLength, oversizedArray.Length);
    }

    [Fact]
    public void Dispose_ShouldNotThrowException()
    {
        // Arrange
        var arrayPool = new ArrayPool<int>(128);

        // Act & Assert
        var exception = Record.Exception(arrayPool.Dispose);
        Assert.Null(exception);
    }

    [Fact]
    public void Dispose_ShouldAllowSafeMultipleCalls()
    {
        // Arrange
        var arrayPool = new ArrayPool<int>(128);

        // Act & Assert
        arrayPool.Dispose();
        var exception = Record.Exception(arrayPool.Dispose);
        Assert.Null(exception);
    }
}
