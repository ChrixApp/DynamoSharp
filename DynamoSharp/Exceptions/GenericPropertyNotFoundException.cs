namespace DynamoSharp.Exceptions;

public class GenericPropertyNotFoundException : Exception
{
    public GenericPropertyNotFoundException() : base("Generic property not found.")
    {
    }

    public GenericPropertyNotFoundException(string message) : base(message)
    {
    }

    public GenericPropertyNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}