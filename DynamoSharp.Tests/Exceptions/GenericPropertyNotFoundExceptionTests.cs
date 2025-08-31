using DynamoSharp.Exceptions;

namespace DynamoSharp.Tests.Exceptions;

public class GenericPropertyNotFoundExceptionTests
{
    [Fact]
    public void IsAssignableFrom_Exception()
    {
        var ex = new GenericPropertyNotFoundException();
        Assert.IsType<GenericPropertyNotFoundException>(ex);
        Assert.IsAssignableFrom<Exception>(ex);
    }

    [Fact]
    public void DefaultConstructor_SetsDefaultMessage()
    {
        var ex = new GenericPropertyNotFoundException();
        Assert.Equal("Generic property not found.", ex.Message);
    }

    [Fact]
    public void Constructor_SetsMessage_WhenMessageProvided()
    {
        var message = "A specific generic property was not found";
        var ex = new GenericPropertyNotFoundException(message);
        Assert.Equal(message, ex.Message);
    }

    [Fact]
    public void Constructor_AllowsEmptyMessage()
    {
        var message = string.Empty;
        var ex = new GenericPropertyNotFoundException(message);
        Assert.Equal(message, ex.Message);
    }

    [Fact]
    public void Constructor_AllowsNullMessage_DefaultMessageIsPresent()
    {
        // Passing null to the base Exception constructor results in the framework default message.
        var ex = new GenericPropertyNotFoundException(null!);
        Assert.NotNull(ex.Message);
        Assert.Contains(nameof(GenericPropertyNotFoundException), ex.Message);
    }

    [Fact]
    public void ToString_IncludesCustomMessage()
    {
        var message = "custom-to-string";
        var ex = new GenericPropertyNotFoundException(message);
        Assert.Contains(message, ex.ToString());
    }
}