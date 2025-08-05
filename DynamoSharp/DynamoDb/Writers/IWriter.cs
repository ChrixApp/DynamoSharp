namespace DynamoSharp.DynamoDb.Writers;

public interface IWriter
{
    [Obsolete("This method is deprecated. Use SaveChangesAsync method instead.")]
    public abstract Task WriteAsync(CancellationToken cancellationToken = default);
    public abstract Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
