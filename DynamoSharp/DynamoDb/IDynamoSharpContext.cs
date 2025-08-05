using DynamoSharp.ChangeTracking;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;
using DynamoSharp.DynamoDb.QueryBuilder;
using DynamoSharp.DynamoDb.Writers;
using EfficientDynamoDb;

namespace DynamoSharp.DynamoDb;

public interface IDynamoSharpContext
{
    TableSchema TableSchema { get; }
    IModelBuilder ModelBuilder { get; }
    IDynamoDbContext DynamoDbContext { get; }
    IChangeTracker ChangeTracker { get; }
    IWriter BatchWriter { get; }
    IWriter TransactWriter { get; }

    IQueryBuilder<TEntity> Query<TEntity>();
    void OnModelCreating(IModelBuilder modelBuilder);
    void Registration();
}
