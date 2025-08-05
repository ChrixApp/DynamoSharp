namespace DynamoSharp.Exceptions;

public class PathNotFoundException : Exception
{
    public PathNotFoundException() : base("Path not found.")
    {
    }

    public PathNotFoundException(string message) : base(message)
    {
    }

    public PathNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
