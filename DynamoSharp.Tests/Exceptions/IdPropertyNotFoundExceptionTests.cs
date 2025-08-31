using DynamoSharp.Exceptions;

namespace DynamoSharp.Tests.Exceptions;

public class IdPropertyNotFoundExceptionTests
{
    [Fact]
    public void IsAssignableFrom_Exception()
    {
        var ex = new IdPropertyNotFoundException("msg");
        Assert.IsType<IdPropertyNotFoundException>(ex);
        Assert.IsAssignableFrom<Exception>(ex);
    }

    [Fact]
    public void Constructor_SetsMessage_WhenMessageProvided()
    {
        var message = "Id property was not found on the model";
        var ex = new IdPropertyNotFoundException(message);
        Assert.Equal(message, ex.Message);
    }

    [Fact]
    public void Constructor_AllowsEmptyMessage()
    {
        var message = string.Empty;
        var ex = new IdPropertyNotFoundException(message);
        Assert.Equal(message, ex.Message);
    }

    [Fact]
    public void Constructor_AllowsNullMessage_DefaultMessageIsPresent()
    {
        // Passing null to the base Exception constructor results in the framework default message.
        var ex = new IdPropertyNotFoundException(null!);
        Assert.NotNull(ex.Message);
        Assert.Contains(nameof(IdPropertyNotFoundException), ex.Message);
    }

    [Fact]
    public void ToString_IncludesCustomMessage()
    {
        var message = "custom-to-string";
        var ex = new IdPropertyNotFoundException(message);
        Assert.Contains(message, ex.ToString());
    }
}