using DynamoSharp.Exceptions;

namespace DynamoSharp.Tests.Exceptions;

public class PartiQLQueryBuilderExceptionTests
{
    [Fact]
    public void BaseException_ShouldPreserveMessage_AndBeException()
    {
        var ex = new PartiQLQueryBuilderException("base message");

        Assert.IsType<PartiQLQueryBuilderException>(ex);
        Assert.IsAssignableFrom<Exception>(ex);
        Assert.Equal("base message", ex.Message);
    }

    [Fact]
    public void AllDerivedExceptions_ShouldInheritFromPartiQLQueryBuilderException_AndPreserveMessage()
    {
        var baseType = typeof(PartiQLQueryBuilderException);
        var assembly = baseType.Assembly;

        var derivedTypes = assembly
            .GetTypes()
            .Where(t => t != baseType && baseType.IsAssignableFrom(t) && !t.IsAbstract)
            .ToArray();

        Assert.NotEmpty(derivedTypes);

        foreach (var t in derivedTypes)
        {
            // Each derived exception has a ctor(string message)
            var instance = Activator.CreateInstance(t, "derived message");
            Assert.NotNull(instance);
            Assert.IsType(t, instance);
            Assert.IsAssignableFrom<PartiQLQueryBuilderException>(instance);

            var message = ((Exception)instance).Message;
            Assert.Equal("derived message", message);

            // Verify it can be caught as the base PartiQLQueryBuilderException
            var caught = false;
            try
            {
                throw (Exception)instance!;
            }
            catch (PartiQLQueryBuilderException pe)
            {
                caught = true;
                Assert.IsType(t, pe);
                Assert.Equal("derived message", pe.Message);
            }

            Assert.True(caught, $"Exception of type {t.FullName} was not caught as PartiQLQueryBuilderException");
        }
    }
}