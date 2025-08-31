using DynamoSharp.Exceptions;

namespace DynamoSharp.Tests.Exceptions;

public class PathNotFoundExceptionTests
{
    [Fact]
    public void IsAssignableFrom_Exception()
    {
        var ex = new PathNotFoundException("msg");
        Assert.IsType<PathNotFoundException>(ex);
        Assert.IsAssignableFrom<Exception>(ex);
    }

    [Fact]
    public void Constructor_SetsMessage_WhenMessageProvided()
    {
        var message = "Path not found";
        var ex = new PathNotFoundException(message);
        Assert.Equal(message, ex.Message);
    }

    [Fact]
    public void Constructor_AllowsEmptyMessage()
    {
        var message = string.Empty;
        var ex = new PathNotFoundException(message);
        Assert.Equal(message, ex.Message);
    }

    [Fact]
    public void Constructor_AllowsNullMessage_DefaultMessageIsPresent()
    {
        var ex = new PathNotFoundException(null!);
        Assert.NotNull(ex.Message);
        Assert.Contains(nameof(PathNotFoundException), ex.Message);
    }

    [Fact]
    public void ToString_IncludesCustomMessage()
    {
        var message = "custom-to-string";
        var ex = new PathNotFoundException(message);
        Assert.Contains(message, ex.ToString());
    }
}