namespace DynamoSharp.DynamoDb;

public interface IDynamoDbSet<TEntity>
{
    IDynamoSharpContext DynamoSharpContext { get; }

    void Add(TEntity entity);
    void AddRange(List<TEntity> entities);
    void Remove(TEntity entity);
    void RemoveRange(List<TEntity> entities);
    Type GetGenericType();
}