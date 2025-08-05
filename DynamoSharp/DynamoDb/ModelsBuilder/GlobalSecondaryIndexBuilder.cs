using System.Linq.Expressions;
using DynamoSharp.Exceptions;

namespace DynamoSharp.DynamoDb.ModelsBuilder;

public class GlobalSecondaryIndexBuilder<TEntity>
{
    public IList<GlobalSecondaryIndex> GSI { get; }

    public GlobalSecondaryIndexBuilder(IList<GlobalSecondaryIndex> properties)
    {
        GSI = properties;
    }

    public GlobalSecondaryIndexBuilder<TEntity> Include(Expression<Func<TEntity, object>> memberLamda, string prefix = "")
    {
        var path = ExpressionHandler.FindPath(memberLamda);
        Thrower.ThrowIfNull<PathNotFoundException>(path);
        GSI.Add(new GlobalSecondaryIndex(path, prefix));
        return this;
    }
}
