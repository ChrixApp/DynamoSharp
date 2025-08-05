using System.Linq.Expressions;
using System.Reflection;
using DynamoSharp.Exceptions;

namespace DynamoSharp.DynamoDb.ModelsBuilder;

public class EntityTypeBuilder<TEntity> : IEntityTypeBuilder
{
    public string IdName { get; private set; }
    public IDictionary<string, string> PartitionKey { get; private set; }
    public IDictionary<string, string> SortKey { get; private set; }
    public IDictionary<string, IList<GlobalSecondaryIndex>> GlobalSecondaryIndexPartitionKey { get; private set; }
    public IDictionary<string, IList<GlobalSecondaryIndex>> GlobalSecondaryIndexSortKey { get; private set; }

    public IDictionary<string, Type> OneToMany { get; private set; }
    public IDictionary<string, Type> ManyToMany { get; private set; }

    public bool Versioning { get; private set; }

    public EntityTypeBuilder()
    {
        IdName = "Id";
        PartitionKey = new Dictionary<string, string>();
        SortKey = new Dictionary<string, string>();
        OneToMany = new Dictionary<string, Type>();
        ManyToMany = new Dictionary<string, Type>();
        GlobalSecondaryIndexPartitionKey = new Dictionary<string, IList<GlobalSecondaryIndex>>();
        GlobalSecondaryIndexSortKey = new Dictionary<string, IList<GlobalSecondaryIndex>>();
    }

    public void HasId(Expression<Func<TEntity, object>> memberLamda)
    {
        var path = ExpressionHandler.FindPath(memberLamda);
        IdName = path ?? string.Empty;
    }

    public void HasVersioning()
    {
        Versioning = true;
    }

    public KeyModelBuilder<TEntity> HasPartitionKey(Expression<Func<TEntity, object>> memberLamda, string prefix = "")
    {
        var path = ExpressionHandler.FindPath(memberLamda);
        Thrower.ThrowIfNull<PathNotFoundException>(path);
        PartitionKey.Add(path, prefix);
        return new KeyModelBuilder<TEntity>(PartitionKey);
    }

    public KeyModelBuilder<TEntity> HasSortKey(Expression<Func<TEntity, object>> memberLamda, string prefix = "")
    {
        var path = ExpressionHandler.FindPath(memberLamda);
        Thrower.ThrowIfNull<PathNotFoundException>(path);
        SortKey.Add(path, prefix);
        return new KeyModelBuilder<TEntity>(SortKey);
    }

    public GlobalSecondaryIndexBuilder<TEntity> HasGlobalSecondaryIndexPartitionKey(string name, Expression<Func<TEntity, object>> memberLamda, string prefix = "")
    {
        var gsi = new List<GlobalSecondaryIndex>();
        var path = ExpressionHandler.FindPath(memberLamda);
        Thrower.ThrowIfNull<PathNotFoundException>(path);
        gsi.Add(new GlobalSecondaryIndex(path, prefix));
        GlobalSecondaryIndexPartitionKey.Add(name, gsi);
        return new GlobalSecondaryIndexBuilder<TEntity>(gsi);
    }

    public GlobalSecondaryIndexBuilder<TEntity> HasGlobalSecondaryIndexPartitionKey(string name, string value)
    {
        var gsi = new List<GlobalSecondaryIndex>();
        gsi.Add(new GlobalSecondaryIndex(value));
        GlobalSecondaryIndexPartitionKey.Add(name, gsi);
        return new GlobalSecondaryIndexBuilder<TEntity>(gsi);
    }

    public GlobalSecondaryIndexBuilder<TEntity> HasGlobalSecondaryIndexSortKey(string name, Expression<Func<TEntity, object>> memberLamda, string prefix = "")
    {
        var gsi = new List<GlobalSecondaryIndex>();
        var path = ExpressionHandler.FindPath(memberLamda);
        Thrower.ThrowIfNull<PathNotFoundException>(path);
        gsi.Add(new GlobalSecondaryIndex(path, prefix));
        GlobalSecondaryIndexSortKey.Add(name, gsi);
        return new GlobalSecondaryIndexBuilder<TEntity>(gsi);
    }

    public GlobalSecondaryIndexBuilder<TEntity> HasGlobalSecondaryIndexSortKey(string name, string value)
    {
        var gsi = new List<GlobalSecondaryIndex>();
        gsi.Add(new GlobalSecondaryIndex(value));
        GlobalSecondaryIndexSortKey.Add(name, gsi);
        return new GlobalSecondaryIndexBuilder<TEntity>(gsi);
    }

    public virtual void HasOneToMany<TRelatedEntity>(
        Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression) where TRelatedEntity : class
    {
        var memberExpression = navigationExpression.Body as MemberExpression;

        if ( memberExpression is not null && memberExpression.Member is not null && memberExpression.Member is PropertyInfo listProperty)
        {
            var genericProperty = listProperty.PropertyType.GetGenericArguments().FirstOrDefault();
            Thrower.ThrowIfNull<GenericPropertyNotFoundException>(genericProperty);
            OneToMany.Add(listProperty.Name, genericProperty);
        }
    }

    public virtual void HasManyToMany<TRelatedEntity>(
        Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression) where TRelatedEntity : class
    {
        var memberExpression = navigationExpression.Body as MemberExpression;

        if (memberExpression is not null && memberExpression.Member is not null && memberExpression.Member is PropertyInfo listProperty)
        {
            var genericProperty = listProperty.PropertyType.GetGenericArguments().FirstOrDefault();
            Thrower.ThrowIfNull<GenericPropertyNotFoundException>(genericProperty);
            ManyToMany.Add(listProperty.Name, genericProperty);
        }
    }
}
