using System.Linq.Expressions;
using DynamoSharp.Exceptions;

namespace DynamoSharp.DynamoDb.ModelsBuilder;

public class KeyModelBuilder<TEntity>
{
    public IDictionary<string, string> Properties { get; }

    public KeyModelBuilder(IDictionary<string, string> properties)
    {
        Properties = properties;
    }

    public KeyModelBuilder<TEntity> Include(Expression<Func<TEntity, object>> memberLamda, string prefix = "")
    {
        var path = ExpressionHandler.FindPath(memberLamda);
        Thrower.ThrowIfNull<PathNotFoundException>(path);
        Properties.Add(path, prefix);
        return this;
    }
}
