using DynamoSharp.Exceptions;

namespace DynamoSharp.Tests.Exceptions;

public class MethodCallArgumentNullExceptionTests
{
    [Fact]
    public void Constructor_ShouldPreserveMessage_And_InheritFromBase()
    {
        var message = "argument null occurred";
        var ex = new MethodCallArgumentNullException(message);

        Assert.IsType<MethodCallArgumentNullException>(ex);
        Assert.IsAssignableFrom<PartiQLQueryBuilderException>(ex);
        Assert.IsAssignableFrom<Exception>(ex);
        Assert.Equal(message, ex.Message);
    }

    [Fact]
    public void ThrownException_CanBeCaughtAsPartiQLQueryBuilderException()
    {
        var message = "thrown message";
        var caught = false;

        try
        {
            throw new MethodCallArgumentNullException(message);
        }
        catch (PartiQLQueryBuilderException pe)
        {
            caught = true;
            Assert.IsType<MethodCallArgumentNullException>(pe);
            Assert.Equal(message, pe.Message);
        }

        Assert.True(caught, "Exception was not caught as PartiQLQueryBuilderException");
    }
}