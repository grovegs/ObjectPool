using System.Diagnostics.CodeAnalysis;

using GroveGames.ObjectPool.Pools;

namespace GroveGames.ObjectPool.Tests.Pools;

public class FixedSizeArrayPoolTests
{
    public class FixedSizeArrayPoolAccessor
    {
        public static Dictionary<T, ObjectPool<T[]>> ObjectPoolsBySizes<T>(FixedSizeArrayPool<T> fixedSizeArrayPool) where T : notnull
        {
            var fieldInfo = typeof(FixedSizeArrayPool<T>).GetField("_objectPoolsBySizes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (Dictionary<T, ObjectPool<T[]>>)fieldInfo?.GetValue(fixedSizeArrayPool)!;
        }
    }

    [Fact]
    public void Get_ShouldReturnArrayOfRequestedSize()
    {
        // Arrange
        var arrayPool = new FixedSizeArrayPool<int>();
        int size = 128;

        // Act
        var array = arrayPool.Get(size);

        // Assert
        Assert.NotNull(array);
        Assert.Equal(size, array.Length);
    }

    [Fact]
    public void Get_ShouldReuseArray_WhenReturnedToPool()
    {
        // Arrange
        var arrayPool = new FixedSizeArrayPool<int>();
        int size = 128;

        // Act
        var array1 = arrayPool.Get(size);
        arrayPool.Return(array1);
        var array2 = arrayPool.Get(size);

        // Assert
        Assert.Same(array1, array2);
    }

    [Fact]
    public void DisposableGet_ShouldReturnArrayAndDisposeCorrectly()
    {
        // Arrange
        var arrayPool = new FixedSizeArrayPool<int>();
        int size = 128;

        // Act
        int[] array;
        using (arrayPool.Get(size, out array))
        {
            Assert.NotNull(array);
            Assert.Equal(size, array.Length);
        }

        // Assert
        var reusedArray = arrayPool.Get(size);
        Assert.Same(array, reusedArray);
    }

    [Fact]
    public void Dispose_ShouldClearAllPools()
    {
        // Arrange
        var arrayPool = new FixedSizeArrayPool<int>();
        int size = 128;

        // Act
        var array = arrayPool.Get(size);
        arrayPool.Return(array);
        arrayPool.Dispose();

        // Assert
        var poolsBySizes = FixedSizeArrayPoolAccessor.ObjectPoolsBySizes(arrayPool);

        // Assert
        Assert.Empty(poolsBySizes);
    }
}