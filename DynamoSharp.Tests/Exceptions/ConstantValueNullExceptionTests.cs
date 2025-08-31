using System;
using DynamoSharp.Exceptions;
using Xunit;

namespace DynamoSharp.Tests.Exceptions;

public class ConstantValueNullExceptionTests
{
    [Fact]
    public void Constructor_ShouldPreserveMessage_And_InheritFromBase()
    {
        var message = "constant value is null";
        var ex = new ConstantValueNullException(message);

        Assert.IsType<ConstantValueNullException>(ex);
        Assert.IsAssignableFrom<PartiQLQueryBuilderException>(ex);
        Assert.IsAssignableFrom<Exception>(ex);
        Assert.Equal(message, ex.Message);
    }

    [Fact]
    public void ThrownException_CanBeCaughtAsPartiQLQueryBuilderException()
    {
        var message = "thrown constant value null";
        var caught = false;

        try
        {
            throw new ConstantValueNullException(message);
        }
        catch (PartiQLQueryBuilderException pe)
        {
            caught = true;
            Assert.IsType<ConstantValueNullException>(pe);
            Assert.Equal(message, pe.Message);
        }

        Assert.True(caught, "Exception was not caught as PartiQLQueryBuilderException");
    }
}