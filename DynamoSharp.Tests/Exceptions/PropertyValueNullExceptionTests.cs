using DynamoSharp.Exceptions;

namespace DynamoSharp.Tests;

public class PropertyValueNullExceptionTests
{
    [Fact]
    public void IsAssignableFrom_Exception()
    {
        var ex = new PropertyValueNullException("msg");
        Assert.IsType<PropertyValueNullException>(ex);
        Assert.IsAssignableFrom<Exception>(ex);
    }

    [Fact]
    public void Constructor_SetsMessage_WhenMessageProvided()
    {
        var message = "Property value cannot be null";
        var ex = new PropertyValueNullException(message);
        Assert.Equal(message, ex.Message);
    }

    [Fact]
    public void Constructor_AllowsEmptyMessage()
    {
        var message = string.Empty;
        var ex = new PropertyValueNullException(message);
        Assert.Equal(message, ex.Message);
    }

    [Fact]
    public void Constructor_AllowsNullMessage_DefaultMessageIsPresent()
    {
        var ex = new PropertyValueNullException(null!);
        Assert.NotNull(ex.Message);
        Assert.Contains(nameof(PropertyValueNullException), ex.Message);
    }

    [Fact]
    public void ToString_IncludesCustomMessage()
    {
        var message = "custom-to-string";
        var ex = new PropertyValueNullException(message);
        Assert.Contains(message, ex.ToString());
    }
}
