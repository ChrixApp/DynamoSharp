using DynamoSharp.Exceptions;
using FluentAssertions;

namespace DynamoSharp.Tests.Exceptions;

public class ThrowerTests
{
    private class TestExceptionWithStringCtor : Exception
    {
        public TestExceptionWithStringCtor(string message) : base(message) { }
    }

    private class TestExceptionWithoutStringCtor : Exception
    {
        public TestExceptionWithoutStringCtor() { }
    }

    [Fact]
    public void ThrowIfNull_DoesNotThrow_WhenArgumentIsNotNull()
    {
        Action act = () => Thrower.ThrowIfNull<TestExceptionWithStringCtor>(new object(), "ignored");
        act.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfNull_UsesParameterlessConstructor_WhenMessageIsNull()
    {
        Action act = () => Thrower.ThrowIfNull<MissingPartitionKeyException>(null);

        act.Should().Throw<MissingPartitionKeyException>()
           .Where(e => e.Message == "Partition key is required");
    }

    [Fact]
    public void ThrowIfNull_ThrowsSpecifiedException_WhenArgumentIsNull_AndCtorExists()
    {
        Action act = () => Thrower.ThrowIfNull<TestExceptionWithStringCtor>(null, "expected message");
        act.Should().Throw<TestExceptionWithStringCtor>().Where(e => e.Message == "expected message");
    }

    [Fact]
    public void ThrowIfNull_ThrowsInvalidOperationException_WhenExceptionTypeLacksStringCtor()
    {
        var exceptionTypeFullName = typeof(TestExceptionWithoutStringCtor).FullName;
        Action act = () => Thrower.ThrowIfNull<TestExceptionWithoutStringCtor>(null, "ignored");
        act.Should().Throw<InvalidOperationException>().Where(e => e.Message.Contains(exceptionTypeFullName));
    }
}