using DynamoSharp.Exceptions;

namespace DynamoSharp.Tests.Exceptions
{
    public class MemberExpressionClosureNullExceptionTests
    {
        [Fact]
        public void Constructor_ShouldPreserveMessage_And_InheritFromBase()
        {
            var message = "member closure null occurred";
            var ex = new MemberExpressionClosureNullException(message);

            Assert.IsType<MemberExpressionClosureNullException>(ex);
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
                throw new MemberExpressionClosureNullException(message);
            }
            catch (PartiQLQueryBuilderException pe)
            {
                caught = true;
                Assert.IsType<MemberExpressionClosureNullException>(pe);
                Assert.Equal(message, pe.Message);
            }

            Assert.True(caught, "Exception was not caught as PartiQLQueryBuilderException");
        }
    }
}
