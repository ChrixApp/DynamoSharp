using DynamoSharp.Exceptions;

namespace DynamoSharp.Tests.Exceptions;

public class JTokenNullExceptionTests
{
    [Fact]
    public void IsAssignableFrom_Exception()
    {
        var ex = new JTokenNullException();
        Assert.IsType<JTokenNullException>(ex);
        Assert.IsAssignableFrom<Exception>(ex);
    }

    [Fact]
    public void DefaultConstructor_MessageIsNotNull_AndContainsTypeName()
    {
        var ex = new JTokenNullException();
        Assert.NotNull(ex.Message);
        Assert.Contains(nameof(JTokenNullException), ex.Message);
    }

    [Fact]
    public void ToString_IncludesExceptionTypeName()
    {
        var ex = new JTokenNullException();
        Assert.Contains(nameof(JTokenNullException), ex.ToString());
    }
}