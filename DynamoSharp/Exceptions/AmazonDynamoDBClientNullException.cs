namespace DynamoSharp.Exceptions;

public class AmazonDynamoDBClientNullException : Exception
{
    public AmazonDynamoDBClientNullException(string? message) : base(message)
    {
    }
}
