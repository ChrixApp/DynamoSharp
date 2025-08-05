namespace DynamoSharp.Exceptions;

public class DynamoDbSetNotFoundException : Exception
{
    public DynamoDbSetNotFoundException()
    {
    }

    public DynamoDbSetNotFoundException(string? message) : base(message)
    {
    }

    public DynamoDbSetNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}