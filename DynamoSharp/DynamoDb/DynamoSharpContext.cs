using EfficientDynamoDb;
using System.Collections.Concurrent;
using System.Reflection;
using DynamoSharp.ChangeTracking;
using DynamoSharp.Converters.Entities;
using DynamoSharp.Converters.Objects;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;
using DynamoSharp.DynamoDb.QueryBuilder;
using DynamoSharp.DynamoDb.Writers;

namespace DynamoSharp.DynamoDb;

public partial class DynamoSharpContext : IDynamoSharpContext
{
    private readonly IDynamoDbContext _dynamoDbContext;
    private readonly IEntityConverter _entityConverter;
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _entityPropertiesCache = new();

    public TableSchema TableSchema { get; }
    public IModelBuilder ModelBuilder { get; }
    public IChangeTracker ChangeTracker { get; }
    public IDynamoDbContext DynamoDbContext => _dynamoDbContext;
    public static ConcurrentDictionary<Type, PropertyInfo[]> EntityPropertiesCache => _entityPropertiesCache;
    public IWriter BatchWriter { get; }
    public IWriter TransactWriter { get; }

    public DynamoSharpContext(IDynamoDbContextAdapter dynamoDbContextAdapter, TableSchema tableSchema)
    {
        _dynamoDbContext = dynamoDbContextAdapter.DynamoDbContext;
        _entityConverter = new EntityConverter(tableSchema);
        TableSchema = tableSchema;
        ModelBuilder = new ModelBuilder();
        ChangeTracker = new ChangeTracker(tableSchema, ModelBuilder);
        BatchWriter = new BatchWriter(_entityConverter, _dynamoDbContext, TableSchema, ModelBuilder, ChangeTracker);
        TransactWriter = new TransactWriter(_entityConverter, _dynamoDbContext, TableSchema, ModelBuilder, ChangeTracker);
    }

    public virtual void OnModelCreating(IModelBuilder modelBuilder) { }

    public virtual void Registration()
    {
        var props = GetType().GetProperties()
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(IDynamoDbSet<>))
            .ToList();
        if (props == null) return;

        foreach (var prop in props)
        {
            var dbSetEntityType = prop.PropertyType.GetGenericArguments()[0];
            EntityPropertiesCache.GetOrAdd(dbSetEntityType, t => t.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
            var dctSetEntityType = typeof(DynamoDbSet<>).MakeGenericType(dbSetEntityType);
            var dctSet = Activator.CreateInstance(dctSetEntityType, this);
            ReflectionUtils.SetValue(this, GetType(), prop.Name, dctSet);
        }
    }

    public IQueryBuilder<TEntity> Query<TEntity>()
    {
        return new Query<TEntity>.Builder(this, TableSchema);
    }
}
