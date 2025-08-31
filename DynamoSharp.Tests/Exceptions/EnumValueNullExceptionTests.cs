using DynamoSharp.Exceptions;

namespace DynamoSharp.Tests.Exceptions;

public class EnumValueNullExceptionTests
{
    [Fact]
    public void Constructor_ShouldPreserveMessage_And_InheritFromBase()
    {
        var message = "enum value is null";
        var ex = new EnumValueNullException(message);

        Assert.IsType<EnumValueNullException>(ex);
        Assert.IsAssignableFrom<PartiQLQueryBuilderException>(ex);
        Assert.IsAssignableFrom<Exception>(ex);
        Assert.Equal(message, ex.Message);
    }

    [Fact]
    public void ThrownException_CanBeCaughtAsPartiQLQueryBuilderException()
    {
        var message = "thrown enum value null";
        var caught = false;

        try
        {
            throw new EnumValueNullException(message);
        }
        catch (PartiQLQueryBuilderException pe)
        {
            caught = true;
            Assert.IsType<EnumValueNullException>(pe);
            Assert.Equal(message, pe.Message);
        }

        Assert.True(caught, "Exception was not caught as PartiQLQueryBuilderException");
    }
}