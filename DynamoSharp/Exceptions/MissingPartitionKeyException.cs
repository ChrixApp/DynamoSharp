namespace DynamoSharp.Exceptions;

public class MissingPartitionKeyException : Exception
{
    public MissingPartitionKeyException() : base("Partition key is required")
    {
    }

    public MissingPartitionKeyException(string message) : base(message)
    {
    }
}