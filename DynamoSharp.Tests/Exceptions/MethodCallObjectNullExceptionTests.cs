using DynamoSharp.Exceptions;

namespace DynamoSharp.Tests.Exceptions;

public class MethodCallObjectNullExceptionTests
{
    [Fact]
    public void Constructor_ShouldPreserveMessage_And_InheritFromBase()
    {
        var message = "object null occurred";
        var ex = new MethodCallObjectNullException(message);

        Assert.IsType<MethodCallObjectNullException>(ex);
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
            throw new MethodCallObjectNullException(message);
        }
        catch (PartiQLQueryBuilderException pe)
        {
            caught = true;
            Assert.IsType<MethodCallObjectNullException>(pe);
            Assert.Equal(message, pe.Message);
        }

        Assert.True(caught, "Exception was not caught as PartiQLQueryBuilderException");
    }
}