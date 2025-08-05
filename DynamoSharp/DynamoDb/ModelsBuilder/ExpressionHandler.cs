using System.Linq.Expressions;
using DynamoSharp.Exceptions;

namespace DynamoSharp.DynamoDb.ModelsBuilder;

public static class ExpressionHandler
{
    public static string? FindPath<TEntity>(Expression<Func<TEntity, object>> memberLamda)
    {
        if (memberLamda.Body.NodeType == ExpressionType.MemberAccess)
        {
            var expressionString = memberLamda.Body.ToString();
            return expressionString.Substring(expressionString.IndexOf(".") + 1);
        }
        else if (memberLamda.Body.NodeType == ExpressionType.Convert || memberLamda.Body.NodeType == ExpressionType.TypeAs)
        {
            var unaryExpression = (UnaryExpression)memberLamda.Body;
            var expressionString = unaryExpression.Operand.ToString();
            return expressionString.Substring(expressionString.IndexOf(".") + 1);
        }

        throw new PathNotFoundException("Invalid expression type");
    }
}
