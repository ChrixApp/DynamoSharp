namespace DynamoSharp.Exceptions;

public class EntityTypeBuilderNotFoundException : Exception
{
    public EntityTypeBuilderNotFoundException(Type entityType) : base($"Entity type builder not found for {entityType.Name}")
    {
    }

    public EntityTypeBuilderNotFoundException(string entityType) : base($"Entity type builder not found for {entityType}")
    {
    }
}
