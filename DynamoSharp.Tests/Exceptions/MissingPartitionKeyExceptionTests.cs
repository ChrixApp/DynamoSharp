using DynamoSharp.Exceptions;

namespace DynamoSharp.Tests.Exceptions;

public class MissingPartitionKeyExceptionTests
{
    [Fact]
    public void IsAssignableFrom_Exception()
    {
        var ex = new MissingPartitionKeyException();
        Assert.IsType<MissingPartitionKeyException>(ex);
        Assert.IsAssignableFrom<Exception>(ex);
    }

    [Fact]
    public void DefaultConstructor_SetsDefaultMessage()
    {
        var ex = new MissingPartitionKeyException();
        Assert.Equal("Partition key is required", ex.Message);
    }

    [Fact]
    public void Constructor_SetsMessage_WhenMessageProvided()
    {
        var message = "Partition key missing for item";
        var ex = new MissingPartitionKeyException(message);
        Assert.Equal(message, ex.Message);
    }

    [Fact]
    public void Constructor_AllowsEmptyMessage()
    {
        var message = string.Empty;
        var ex = new MissingPartitionKeyException(message);
        Assert.Equal(message, ex.Message);
    }

    [Fact]
    public void Constructor_AllowsNullMessage_DefaultMessageIsPresent()
    {
        // Passing null to the base Exception constructor results in the framework default message.
        var ex = new MissingPartitionKeyException(null!);
        Assert.NotNull(ex.Message);
        Assert.Contains(nameof(MissingPartitionKeyException), ex.Message);
    }

    [Fact]
    public void ToString_IncludesCustomMessage()
    {
        var message = "custom-to-string";
        var ex = new MissingPartitionKeyException(message);
        Assert.Contains(message, ex.ToString());
    }
}