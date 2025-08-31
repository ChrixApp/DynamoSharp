using DynamoSharp.Exceptions;

namespace DynamoSharp.Tests.Exceptions;

public class ConstantNullExceptionTests
{
    [Fact]
    public void Constructor_ShouldPreserveMessage_And_InheritFromBase()
    {
        var message = "constant is null";
        var ex = new ConstantNullException(message);

        Assert.IsType<ConstantNullException>(ex);
        Assert.IsAssignableFrom<PartiQLQueryBuilderException>(ex);
        Assert.IsAssignableFrom<Exception>(ex);
        Assert.Equal(message, ex.Message);
    }

    [Fact]
    public void ThrownException_CanBeCaughtAsPartiQLQueryBuilderException()
    {
        var message = "thrown constant null";
        var caught = false;

        try
        {
            throw new ConstantNullException(message);
        }
        catch (PartiQLQueryBuilderException pe)
        {
            caught = true;
            Assert.IsType<ConstantNullException>(pe);
            Assert.Equal(message, pe.Message);
        }

        Assert.True(caught, "Exception was not caught as PartiQLQueryBuilderException");
    }
}
