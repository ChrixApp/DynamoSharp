namespace DynamoSharp.DynamoDb.Writers;

public interface IWriter
{
    public abstract Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
