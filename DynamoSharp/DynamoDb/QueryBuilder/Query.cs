using Amazon.DynamoDBv2.DocumentModel;
using EfficientDynamoDb;
using System.Collections;
using System.Linq.Expressions;
using DynamoSharp.ChangeTracking;
using DynamoSharp.Converters.Objects;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;
using DynamoSharp.DynamoDb.QueryBuilder.PartiQL;
using DynamoSharp.Exceptions;
using Document = EfficientDynamoDb.DocumentModel.Document;
using ExecuteStatementRequest = EfficientDynamoDb.Operations.ExecuteStatement.ExecuteStatementRequest;
using ExecuteStatementResponse = EfficientDynamoDb.Operations.ExecuteStatement.ExecuteStatementResponse;

namespace DynamoSharp.DynamoDb.QueryBuilder;

public class Query<TEntity>
{
    public string TableName { get; private set; }
    public bool AsNoTracking { get; private set; } = false;
    public string? IndexName { get; private set; }
    public PartitionKey? PartitionKey { get; private set; }
    public SortKey? SortKey { get; private set; }
    public Expression<Func<TEntity, bool>>? Filters { get; private set; }
    public int? Limit { get; private set; }
    public bool ConsistentRead { get; private set; } = false;
    public bool ScanIndexForward { get; private set; } = true;

    private Query(string tableName)
    {
        TableName = tableName;
    }

    private void SetPartitionKey(string attributeName, string attributeValue)
    {
        PartitionKey = new PartitionKey(attributeName, attributeValue);
    }

    private void SetSortKey(string attributeName, QueryOperator queryOperator, params string[] attributeValues)
    {
        SortKey = new SortKey(attributeName, queryOperator, attributeValues);
    }

    public class Builder : IQueryBuilder<TEntity>
    {
        private readonly Query<TEntity> _query;
        private readonly IDynamoDbContext _dynamoDbContext;
        private readonly IDynamoSharpContext _dynamoSharpContext;
        private readonly TableSchema _tableSchema;
        private readonly PartiQLQueryBuilder _partiQLQueryBuilder;

        public Query<TEntity> Query => _query;

        public Builder(IDynamoSharpContext dynamoSharpContext, TableSchema tableSchema)
        {
            _query = new Query<TEntity>(tableSchema.TableName);
            _dynamoDbContext = dynamoSharpContext.DynamoDbContext;
            _dynamoSharpContext = dynamoSharpContext;
            _tableSchema = tableSchema;
            _partiQLQueryBuilder = new PartiQLQueryBuilder();
        }

        public IQueryBuilder<TEntity> AsNoTracking(bool asNoTracking = true)
        {
            _query.AsNoTracking = asNoTracking;
            return this;
        }

        public IQueryBuilder<TEntity> IndexName(string indexName)
        {
            _query.IndexName = indexName;
            return this;
        }

        public IQueryBuilder<TEntity> PartitionKey(string attributeValue)
        {
            var partitionKey = string.IsNullOrWhiteSpace(_query.IndexName) ? _tableSchema.PartitionKeyName : _tableSchema.GetGlobalSecondaryIndexPartitionKey(_query.IndexName);
            return PartitionKey(partitionKey, attributeValue);
        }

        private IQueryBuilder<TEntity> PartitionKey(string attributeName, string attributeValue)
        {
            _query.SetPartitionKey(attributeName, attributeValue);
            return this;
        }

        public IQueryBuilder<TEntity> SortKey(QueryOperator queryOperator, params string[] attributeValues)
        {
            var sortKey = string.IsNullOrWhiteSpace(_query.IndexName) ? _tableSchema.SortKeyName : _tableSchema.GetGlobalSecondaryIndexSortKey(_query.IndexName);
            return SortKey(sortKey, queryOperator, attributeValues);
        }

        private IQueryBuilder<TEntity> SortKey(string attributeName, QueryOperator queryOperator, params string[] attributeValues)
        {
            _query.SetSortKey(attributeName, queryOperator, attributeValues);
            return this;
        }

        public IQueryBuilder<TEntity> Filters(Expression<Func<TEntity, bool>> filters)
        {
            _query.Filters = filters;
            return this;
        }

        public IQueryBuilder<TEntity> Limit(int? limit)
        {
            _query.Limit = limit;
            return this;
        }

        public IQueryBuilder<TEntity> ConsistentRead(bool consistentRead = true)
        {
            _query.ConsistentRead = consistentRead;
            return this;
        }

        public IQueryBuilder<TEntity> ScanIndexForward(bool scanIndexForward = false)
        {
            _query.ScanIndexForward = scanIndexForward;
            return this;
        }

        public async Task<TEntity?> ToEntityAsync(CancellationToken cancellationToken = default)
        {
            Validate();
            var items = await ExecuteStatementAsync(cancellationToken);

            if (items.Count == 0) return default;

            if (_dynamoSharpContext.ModelBuilder.Entities[typeof(TEntity)].OneToMany.Count == 0 &&
                _dynamoSharpContext.ModelBuilder.Entities[typeof(TEntity)].ManyToMany.Count == 0 &&
                items.Count > 1)
                throw new InvalidOperationException("The entity has no relationships, but more than one document was found.");

            var rootEntity = RebuildRootEntity(
                items,
                _dynamoSharpContext.TableSchema.PartitionKeyName,
                _dynamoSharpContext.TableSchema.SortKeyName);
            if (rootEntity is null) return default;

            RebuildRelationships(
                rootEntity,
                _dynamoSharpContext.ModelBuilder,
                items,
                _dynamoSharpContext.TableSchema.SortKeyName);

            return rootEntity;
        }

        private async Task<List<Document>> ExecuteStatementAsync(CancellationToken cancellationToken)
        {
            var partiQLQuery = _partiQLQueryBuilder
                .SelectFrom(_query.TableName, _query.IndexName)
                .WithPartitionKey(_query.PartitionKey)
                .WithSortKey(_query.SortKey)
                .WithFilters(_query.Filters)
                .OrderBy(_query.SortKey, !_query.ScanIndexForward)
                .Build();

            var executeStatementRequest = new ExecuteStatementRequest
            {
                Statement = partiQLQuery.Statement,
                Parameters = partiQLQuery.Parameters,
                Limit = _query.Limit,
                ConsistentRead = _query.ConsistentRead
            };

            var items = new List<Document>();
            ExecuteStatementResponse executeStatementResponse;

            do
            {
                executeStatementResponse = await _dynamoDbContext.LowLevel.PartiQL.ExecuteStatementAsync(executeStatementRequest, cancellationToken);
                items.AddRange(executeStatementResponse.Items);
                
                if (_query.Limit.HasValue)
                    executeStatementRequest.Limit -= executeStatementResponse.Items.Count;

                executeStatementRequest.NextToken = executeStatementResponse.NextToken;
            } while (!string.IsNullOrWhiteSpace(executeStatementResponse.NextToken) && (!_query.Limit.HasValue || items.Count < _query.Limit));

            return items;
        }

        private void AsTracking(object entity, Document doc)
        {
            if (_query.AsNoTracking) return;

            EntityChangeTracker entityChangeTracker;
            var entityTypeBuilder = _dynamoSharpContext.ModelBuilder.Entities[entity.GetType()];

            if (entityTypeBuilder.Versioning)
            {
                var version = doc[_tableSchema.VersionName].AsNumberAttribute().ToInt();

                entityChangeTracker = new EntityChangeTracker(
                _dynamoSharpContext.ModelBuilder,
                entity,
                EntityState.Unchanged,
                version);
            }
            else
            {
                entityChangeTracker = new EntityChangeTracker(
                _dynamoSharpContext.ModelBuilder,
                entity,
                EntityState.Unchanged);
            }

            _dynamoSharpContext.ChangeTracker.Track(entityChangeTracker);
        }

        private void AsTracking(object entity, Document doc, object parentEntity)
        {
            if (_query.AsNoTracking) return;

            EntityChangeTracker entityChangeTracker;
            var entityTypeBuilder = _dynamoSharpContext.ModelBuilder.Entities[entity.GetType()];

            if (entityTypeBuilder.Versioning)
            {
                var version = doc[_tableSchema.VersionName].AsNumberAttribute().ToInt();

                entityChangeTracker = new EntityChangeTracker(
                _dynamoSharpContext.ModelBuilder,
                entity,
                EntityState.Unchanged,
                parentEntity,
                version);
            } 
            else
            {
                entityChangeTracker = new EntityChangeTracker(
                _dynamoSharpContext.ModelBuilder,
                entity,
                EntityState.Unchanged,
                parentEntity);
            }

            _dynamoSharpContext.ChangeTracker.Track(entityChangeTracker);
        }

        public async Task<List<TEntity>> ToListAsync(CancellationToken cancellationToken = default)
        {
            Validate();
            var items = await ExecuteStatementAsync(cancellationToken);

            if (items.Count == 0)
                return new List<TEntity>();

            var entities = DocumentsToEntities(items);

            return entities;
        }

        private TEntity RebuildRootEntity(
            IReadOnlyList<Document> documents,
            string partitionKeyName,
            string sortKeyName)
        {
            TEntity rootEntity;
            if (documents.Count == 1)
            {
                rootEntity = (TEntity)ObjectConverter.Instance.ConvertDocumentToObject(documents.Single(), typeof(TEntity));
                AsTracking(rootEntity, documents.Single());
                return rootEntity;
            }

            var rootEntityDoc = documents.Single(document => document[sortKeyName].AsString() == document[partitionKeyName].AsString());
            rootEntity = (TEntity)ObjectConverter.Instance.ConvertDocumentToObject(rootEntityDoc, typeof(TEntity));
            AsTracking(rootEntity, rootEntityDoc);
            return rootEntity;
        }

        private void RebuildRelationships(
            object rootEntity,
            IModelBuilder modelBuilder,
            IReadOnlyList<Document> documents,
            string sortKeyName)
        {
            var rootEntityModelBuilder = modelBuilder.Entities[typeof(TEntity)];

            foreach (var entity in rootEntityModelBuilder.OneToMany)
            {
                FindEntitiesInOneToMany(entity, rootEntity, modelBuilder, sortKeyName, documents);
            }

            foreach (var entity in rootEntityModelBuilder.ManyToMany)
            {
                FindEntitiesInManyToMany(entity, rootEntity, modelBuilder, documents);
            }
        }

        private void FindEntitiesInOneToMany(
            KeyValuePair<string, Type> entityKeyValue,
            object parentEntity,
            IModelBuilder modelBuilder,
            string sortKeyName,
            IReadOnlyList<Document> documents)
        {
            var listType = typeof(List<>).MakeGenericType(entityKeyValue.Value);
            var entities = (IList?)Activator.CreateInstance(listType);
            ReflectionUtils.SetValue(parentEntity, parentEntity.GetType(), entityKeyValue.Key, entities);

            var entityTypeBuilder = modelBuilder.Entities[entityKeyValue.Value];
            var sortKeyValue = entityTypeBuilder.SortKey.First().Value;
            var matchingEntities = documents
                .Where(doc => doc.ContainsKey(sortKeyName) && doc[sortKeyName].AsString().StartsWith($"{sortKeyValue}#"))
                .ToList();

            matchingEntities.ForEach(doc =>
            {
                var entity = ObjectConverter.Instance.ConvertDocumentToObject(doc, entityKeyValue.Value);
                if (entity is null) return;
                entities?.Add(entity);
                AsTracking(entity, doc, parentEntity);
            });
        }

        private void FindEntitiesInManyToMany(
            KeyValuePair<string, Type> entityKeyValue,
            object parentEntity,
            IModelBuilder modelBuilder,
            IReadOnlyList<Document> documents)
        {
            var listType = typeof(List<>).MakeGenericType(entityKeyValue.Value);
            var manyToManyList = (IList?)Activator.CreateInstance(listType);
            ReflectionUtils.SetValue(parentEntity, parentEntity.GetType(), entityKeyValue.Key, manyToManyList);

            var entityTypeBuilder = modelBuilder.Entities[entityKeyValue.Value];

            var gsiSortKeyPreName = entityTypeBuilder.GlobalSecondaryIndexSortKey.First().Key;
            var gsiSortKeyPrefix = entityTypeBuilder.GlobalSecondaryIndexSortKey.First().Value[0].Prefix;

            var matchingDocuments = documents
                .Where(doc => doc.ContainsKey(gsiSortKeyPreName) && doc[gsiSortKeyPreName].AsString().StartsWith(gsiSortKeyPrefix))
                .ToList();

            matchingDocuments.ForEach(doc =>
            {
                var entity = ObjectConverter.Instance.ConvertDocumentToObject(doc, entityKeyValue.Value);
                if (entity is null) return;
                manyToManyList?.Add(entity);
                AsTracking(entity, doc, parentEntity);
            });
        }

        private List<TEntity> DocumentsToEntities(List<Document> documents)
        {
            var entities = new List<TEntity>();

            foreach (var document in documents)
            {
                var entity = (TEntity)ObjectConverter.Instance.ConvertDocumentToObject(document, typeof(TEntity));
                if (entity is null) continue;
                AsTracking(entity, document);
                entities.Add(entity);
            }

            return entities;
        }

        private void Validate()
        {
            if (_query.PartitionKey is null)
            {
                throw new MissingPartitionKeyException();
            }
        }
    }
}

