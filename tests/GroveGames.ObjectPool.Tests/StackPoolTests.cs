namespace GroveGames.ObjectPool.Tests;

public sealed class StackPoolTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesPool()
    {
        // Arrange & Act
        using var pool = new StackPool<int>(5, 10);

        // Assert
        Assert.Equal(0, pool.Count);
        Assert.Equal(10, pool.MaxSize);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_NegativeInitialSize_ThrowsArgumentOutOfRangeException(int initialSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new StackPool<int>(initialSize, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_NonPositiveMaxSize_ThrowsArgumentOutOfRangeException(int maxSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new StackPool<int>(5, maxSize));
    }

    [Fact]
    public void Constructor_InitialSizeGreaterThanMaxSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new StackPool<int>(15, 10));
    }

    [Fact]
    public void Rent_EmptyPool_ReturnsNewStack()
    {
        // Arrange
        using var pool = new StackPool<string>(0, 5);

        // Act
        var stack = pool.Rent();

        // Assert
        Assert.NotNull(stack);
        Assert.Empty(stack);
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Return_Stack_AddsToPool()
    {
        // Arrange
        using var pool = new StackPool<string>(0, 5);
        var stack = pool.Rent();

        // Act
        pool.Return(stack);

        // Assert
        Assert.Equal(1, pool.Count);
    }

    [Fact]
    public void Return_StackWithData_ClearsStack()
    {
        // Arrange
        using var pool = new StackPool<int>(0, 5);
        var stack = pool.Rent();
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);

        // Act
        pool.Return(stack);
        var reusedStack = pool.Rent();

        // Assert
        Assert.Same(stack, reusedStack);
        Assert.Empty(reusedStack);
    }

    [Fact]
    public void RentAndReturn_ReusesStacks()
    {
        // Arrange
        using var pool = new StackPool<string>(0, 5);
        var originalStack = pool.Rent();
        pool.Return(originalStack);

        // Act
        var reusedStack = pool.Rent();

        // Assert
        Assert.Same(originalStack, reusedStack);
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Clear_RemovesAllStacks()
    {
        // Arrange
        using var pool = new StackPool<int>(0, 5);
        var stack1 = pool.Rent();
        var stack2 = pool.Rent();
        var stack3 = pool.Rent();
        pool.Return(stack1);
        pool.Return(stack2);
        pool.Return(stack3);

        // Act
        pool.Clear();

        // Assert
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Count_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new StackPool<int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Count);
    }

    [Fact]
    public void MaxSize_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new StackPool<int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.MaxSize);
    }

    [Fact]
    public void Rent_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new StackPool<int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Rent());
    }

    [Fact]
    public void Return_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new StackPool<int>(0, 5);
        var stack = new Stack<int>();
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Return(stack));
    }

    [Fact]
    public void Clear_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var pool = new StackPool<int>(0, 5);
        pool.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => pool.Clear());
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        // Arrange
        var pool = new StackPool<int>(0, 5);

        // Act & Assert
        pool.Dispose();
        pool.Dispose();
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 10)]
    [InlineData(0, 100)]
    public void Constructor_ValidSizeCombinations_CreatesPool(int initialSize, int maxSize)
    {
        // Arrange & Act
        using var pool = new StackPool<int>(initialSize, maxSize);

        // Assert
        Assert.Equal(0, pool.Count);
        Assert.Equal(maxSize, pool.MaxSize);
    }

    [Fact]
    public void RentMultiple_EmptyPool_CreatesMultipleStacks()
    {
        // Arrange
        using var pool = new StackPool<string>(0, 5);

        // Act
        var stack1 = pool.Rent();
        var stack2 = pool.Rent();
        var stack3 = pool.Rent();

        // Assert
        Assert.NotSame(stack1, stack2);
        Assert.NotSame(stack2, stack3);
        Assert.NotSame(stack1, stack3);
        Assert.Empty(stack1);
        Assert.Empty(stack2);
        Assert.Empty(stack3);
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Return_StackWithLIFOOperations_ClearsAllItems()
    {
        // Arrange
        using var pool = new StackPool<int>(0, 5);
        var stack = pool.Rent();
        stack.Push(10);
        stack.Push(20);
        stack.Push(30);
        var last = stack.Pop();

        // Act
        pool.Return(stack);
        var reusedStack = pool.Rent();

        // Assert
        Assert.Same(stack, reusedStack);
        Assert.Empty(reusedStack);
        Assert.Equal(30, last);
    }

    [Fact]
    public void Return_MaxSizeReached_DoesNotAddToPool()
    {
        // Arrange
        using var pool = new StackPool<int>(0, 2);
        var stack1 = new Stack<int>();
        var stack2 = new Stack<int>();
        var stack3 = new Stack<int>();

        // Act
        pool.Return(stack1);
        pool.Return(stack2);
        pool.Return(stack3);

        // Assert
        Assert.Equal(2, pool.Count);
    }

    [Fact]
    public void Clear_EmptyPool_DoesNotThrow()
    {
        // Arrange
        using var pool = new StackPool<int>(0, 5);

        // Act & Assert
        pool.Clear(); // Should not throw
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Return_StackWithReferenceTypes_ClearsCorrectly()
    {
        // Arrange
        using var pool = new StackPool<string>(0, 5);
        var stack = pool.Rent();
        stack.Push("first");
        stack.Push("second");
        stack.Push("third");

        // Act
        pool.Return(stack);
        var reusedStack = pool.Rent();

        // Assert
        Assert.Same(stack, reusedStack);
        Assert.Empty(reusedStack);
    }

    [Fact]
    public void Stack_AfterClear_BehavesLikeNewStack()
    {
        // Arrange
        using var pool = new StackPool<int>(0, 5);
        var stack = pool.Rent();
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);
        pool.Return(stack);

        // Act
        var reusedStack = pool.Rent();
        reusedStack.Push(100);
        reusedStack.Push(200);

        // Assert
        Assert.Same(stack, reusedStack);
        Assert.Equal(2, reusedStack.Count);
        Assert.Equal(200, reusedStack.Pop());
        Assert.Equal(100, reusedStack.Pop());
    }

    [Fact]
    public void Pool_WithLargeDataSet_HandlesClearingCorrectly()
    {
        // Arrange
        using var pool = new StackPool<int>(0, 5);
        var stack = pool.Rent();

        for (int i = 0; i < 1000; i++)
        {
            stack.Push(i);
        }

        // Act
        pool.Return(stack);
        var reusedStack = pool.Rent();

        // Assert
        Assert.Same(stack, reusedStack);
        Assert.Empty(reusedStack);
    }

    [Fact]
    public void Return_StackWithComplexObjects_ClearsCorrectly()
    {
        // Arrange
        using var pool = new StackPool<List<int>>(0, 5);
        var stack = pool.Rent();
        stack.Push(new List<int> { 1, 2, 3 });
        stack.Push(new List<int> { 4, 5, 6 });
        stack.Push(new List<int> { 7, 8, 9 });

        // Act
        pool.Return(stack);
        var reusedStack = pool.Rent();

        // Assert
        Assert.Same(stack, reusedStack);
        Assert.Empty(reusedStack);
    }

    [Fact]
    public void Stack_LIFOBehavior_PreservedAfterClear()
    {
        // Arrange
        using var pool = new StackPool<string>(0, 5);
        var stack = pool.Rent();
        stack.Push("old1");
        stack.Push("old2");
        pool.Return(stack);

        // Act
        var reusedStack = pool.Rent();
        reusedStack.Push("new1");
        reusedStack.Push("new2");
        reusedStack.Push("new3");

        // Assert
        Assert.Same(stack, reusedStack);
        Assert.Equal(3, reusedStack.Count);
        Assert.Equal("new3", reusedStack.Pop());
        Assert.Equal("new2", reusedStack.Pop());
        Assert.Equal("new1", reusedStack.Pop());
    }

    [Fact]
    public void Return_StackAfterPartialPop_ClearsCorrectly()
    {
        // Arrange
        using var pool = new StackPool<int>(0, 5);
        var stack = pool.Rent();
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);
        stack.Push(4);
        stack.Pop();
        stack.Pop();

        // Act
        pool.Return(stack);
        var reusedStack = pool.Rent();

        // Assert
        Assert.Same(stack, reusedStack);
        Assert.Empty(reusedStack);
    }

    [Fact]
    public void Pool_WithValueTypes_HandlesCorrectly()
    {
        // Arrange
        using var pool = new StackPool<DateTime>(0, 5);
        var stack = pool.Rent();
        stack.Push(DateTime.Now);
        stack.Push(DateTime.UtcNow);
        stack.Push(DateTime.Today);

        // Act
        pool.Return(stack);
        var reusedStack = pool.Rent();

        // Assert
        Assert.Same(stack, reusedStack);
        Assert.Empty(reusedStack);
    }

    [Fact]
    public void Stack_PeekOperation_WorksCorrectlyAfterClear()
    {
        // Arrange
        using var pool = new StackPool<string>(0, 5);
        var stack = pool.Rent();
        stack.Push("old");
        pool.Return(stack);

        // Act
        var reusedStack = pool.Rent();
        reusedStack.Push("new");

        // Assert
        Assert.Same(stack, reusedStack);
        Assert.Equal("new", reusedStack.Peek());
        Assert.Single(reusedStack);
    }

    [Fact]
    public void Stack_MixedPushPopOperations_ClearsCorrectly()
    {
        // Arrange
        using var pool = new StackPool<int>(0, 5);
        var stack = pool.Rent();
        stack.Push(1);
        stack.Push(2);
        stack.Pop();
        stack.Push(3);
        stack.Push(4);
        stack.Pop();
        stack.Push(5);

        // Act
        pool.Return(stack);
        var reusedStack = pool.Rent();

        // Assert
        Assert.Same(stack, reusedStack);
        Assert.Empty(reusedStack);
    }

    [Fact]
    public void Stack_ToArrayOperation_WorksCorrectlyAfterClear()
    {
        // Arrange
        using var pool = new StackPool<int>(0, 5);
        var stack = pool.Rent();
        stack.Push(10);
        stack.Push(20);
        var array = stack.ToArray();
        pool.Return(stack);

        // Act
        var reusedStack = pool.Rent();
        reusedStack.Push(100);
        reusedStack.Push(200);
        var newArray = reusedStack.ToArray();

        // Assert
        Assert.Same(stack, reusedStack);
        Assert.Equal(new[] { 20, 10 }, array);
        Assert.Equal(new[] { 200, 100 }, newArray);
    }

    [Fact]
    public void Stack_ContainsOperation_WorksCorrectlyAfterClear()
    {
        // Arrange
        using var pool = new StackPool<string>(0, 5);
        var stack = pool.Rent();
        stack.Push("test");
        pool.Return(stack);

        // Act
        var reusedStack = pool.Rent();
        reusedStack.Push("new");

        // Assert
        Assert.Same(stack, reusedStack);
        Assert.False(reusedStack.Contains("test"));
        Assert.True(reusedStack.Contains("new"));
    }
}