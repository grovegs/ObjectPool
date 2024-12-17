namespace GroveGames.ObjectPool.Tests;

public class DisposablePooledObjectTests
{
    public class TestObject
    {
        public string? Name { get; set; }
    }

    [Fact]
    public void Dispose_ShouldInvokeOnReturn_WithCorrectPooledObject()
    {
        // Arrange
        var mockOnReturn = new Mock<Action<TestObject>>();
        var testObject = new TestObject();
        var disposablePooledObject = new DisposablePooledObject<TestObject>(mockOnReturn.Object, testObject);

        // Act
        disposablePooledObject.Dispose();

        // Assert
        mockOnReturn.Verify(action => action.Invoke(testObject), Times.Once);
    }

    [Fact]
    public void Dispose_ShouldWorkForMultipleInstances()
    {
        // Arrange
        var mockOnReturn = new Mock<Action<TestObject>>();
        var testObject1 = new TestObject();
        var testObject2 = new TestObject();
        var disposablePooledObject1 = new DisposablePooledObject<TestObject>(mockOnReturn.Object, testObject1);
        var disposablePooledObject2 = new DisposablePooledObject<TestObject>(mockOnReturn.Object, testObject2);

        // Act
        disposablePooledObject1.Dispose();
        disposablePooledObject2.Dispose();

        // Assert
        mockOnReturn.Verify(action => action.Invoke(testObject1), Times.Once);
        mockOnReturn.Verify(action => action.Invoke(testObject2), Times.Once);
    }
}