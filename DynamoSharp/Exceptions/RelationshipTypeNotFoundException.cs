namespace DynamoSharp.Exceptions;

public class RelationshipTypeNotFoundException : Exception
{
    public RelationshipTypeNotFoundException(string message) : base(message) { }
}
