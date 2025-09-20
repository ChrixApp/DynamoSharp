using Amazon.DynamoDBv2.DocumentModel;
using System.Linq.Expressions;

namespace DynamoSharp.DynamoDb.QueryBuilder;

public interface IQueryBuilder<TEntity>
{
    Query<TEntity> Query { get; }

    IQueryBuilder<TEntity> AsNoTracking(bool asNoTracking = true);
    Task<TEntity?> ToEntityAsync(CancellationToken cancellationToken = default);
    Task<List<TEntity>> ToListAsync(CancellationToken cancellationToken = default);
    IQueryBuilder<TEntity> ConsistentRead(bool consistentRead = true);
    IQueryBuilder<TEntity> Filters(Expression<Func<TEntity, bool>> filters);
    IQueryBuilder<TEntity> IndexName(string indexName);
    IQueryBuilder<TEntity> Limit(int? limit);
    IQueryBuilder<TEntity> PartitionKey(string attributeValue);
    IQueryBuilder<TEntity> ScanIndexForward(bool scanIndexForward = false);
    IQueryBuilder<TEntity> SortKey(QueryOperator queryOperator, params object[] attributeValues);
}