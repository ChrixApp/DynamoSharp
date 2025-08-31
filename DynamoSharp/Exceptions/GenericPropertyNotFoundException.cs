namespace DynamoSharp.Exceptions;

public class GenericPropertyNotFoundException : Exception
{
    public GenericPropertyNotFoundException() : base("Generic property not found.")
    {
    }

    public GenericPropertyNotFoundException(string message) : base(message)
    {
    }
}