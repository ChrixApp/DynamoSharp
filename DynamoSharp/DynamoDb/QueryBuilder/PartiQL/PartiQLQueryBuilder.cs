using Amazon.DynamoDBv2.DocumentModel;
using EfficientDynamoDb.DocumentModel;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DynamoSharp.DynamoDb.QueryBuilder.PartiQL.Filter;
using DynamoSharp.Exceptions;
using AttributeValue = EfficientDynamoDb.DocumentModel.AttributeValue;
using Expression = System.Linq.Expressions.Expression;

namespace DynamoSharp.DynamoDb.QueryBuilder.PartiQL;

public class PartiQLQueryBuilder
{
    private const string QuestionMark = "?";
    private readonly StringBuilder _statement = new();
    private readonly List<AttributeValue> _parameters = new();

    public PartiQLQueryBuilder SelectFrom(string tableName, string? indexName = null)
    {
        if (string.IsNullOrWhiteSpace(indexName))
        {
            _statement.Append($"SELECT * FROM \"{tableName}\" WHERE ");
        }
        else
        {
            _statement.Append($"SELECT * FROM \"{tableName}\".\"{indexName}\" WHERE ");
        }
        return this;
    }

    public PartiQLQueryBuilder WithPartitionKey(PartitionKey? partitionKey)
    {
        Thrower.ThrowIfNull<MissingPartitionKeyException>(partitionKey);
        _statement.Append($"{partitionKey.AttributeName} = {QuestionMark}");
        _parameters.Add(new StringAttributeValue(partitionKey.AttributeValue));
        return this;
    }

    public PartiQLQueryBuilder WithSortKey(SortKey? sortKey)
    {
        if (sortKey is null) return this;

        _statement.Append(" AND ");

        if (sortKey.Operator is not QueryOperator.BeginsWith && sortKey.Operator is not QueryOperator.Between)
        {
            _statement.Append($"{sortKey.AttributeName}{ParseQueryOperator(sortKey.Operator)}{QuestionMark}");
            AddParameters(sortKey.AttributeValues[0]);
            return this;
        }

        if (sortKey.Operator is QueryOperator.BeginsWith)
        {
            _statement.Append($"begins_with({sortKey.AttributeName}, {QuestionMark})");
            AddParameters(sortKey.AttributeValues[0]);
            return this;
        }

        _statement.Append($"{sortKey.AttributeName} between {QuestionMark} AND {QuestionMark}");
        AddParameters(sortKey.AttributeValues[0]);
        AddParameters(sortKey.AttributeValues[1]);
        return this;
    }

    private void AddParameters(object attributeValue)
    {
        if (attributeValue is null)
            _parameters.Add(new NullAttributeValue());
        else if (attributeValue is string strValue)
            _parameters.Add(new StringAttributeValue(strValue));
        else if (attributeValue is Guid guidValue)
            _parameters.Add(new StringAttributeValue(guidValue.ToString()));
        else if (attributeValue is DateTime dateTimeValue)
            _parameters.Add(new StringAttributeValue(dateTimeValue.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK")));
        else
            _parameters.Add(new NumberAttributeValue(attributeValue.ToString() ?? throw new ConstantValueNullException("Constant value string is null.")));
    }

    public PartiQLQueryBuilder WithFilters(Expression? filters)
    {
        if (filters is null) return this;
        _statement.Append(" AND ");
        ParseExpression(filters);
        return this;
    }

    public PartiQLQueryBuilder OrderBy(SortKey? sortKey, bool descending = false)
    {
        if (sortKey is not null)
        {
            _statement.Append($" ORDER BY {sortKey.AttributeName}");
        }
        if (descending)
        {
            _statement.Append(" DESC");
        }
        return this;
    }

    public PartiQLQuery Build()
    {
        return new PartiQLQuery(_statement.ToString(), _parameters);
    }

    private static string ParseQueryOperator(QueryOperator queryOperator)
    {
        return queryOperator switch
        {
            QueryOperator.Equal => " = ",
            QueryOperator.GreaterThan => " > ",
            QueryOperator.GreaterThanOrEqual => " >= ",
            QueryOperator.LessThan => " < ",
            QueryOperator.LessThanOrEqual => " <= ",
            QueryOperator.BeginsWith => "begins_with({0}, ?)",
            QueryOperator.Between => " between ",
            _ => throw new ArgumentOutOfRangeException(nameof(queryOperator), queryOperator, null)
        };
    }

    private static bool ContainsSubnodeBinaryExpression(Expression expr)
    {
        if (expr is BinaryExpression binExpr)
        {
            return binExpr.Left is BinaryExpression || binExpr.Right is BinaryExpression;
        }
        return false;
    }

    private void ParseExpression(Expression expression, bool isParentBinary = false, ExpressionType? parentNodeType = null, Type? enumType = null)
    {
        if (expression is LambdaExpression lambdaExpression)
        {
            ParseExpression(lambdaExpression.Body, isParentBinary, parentNodeType, enumType);
        }
        else if (expression is BinaryExpression binaryExpression)
        {
            bool needsParentheses = ContainsSubnodeBinaryExpression(binaryExpression);

            if (needsParentheses) _statement.Append("(");

            enumType = GetEnumType(binaryExpression.Right);
            ParseExpression(binaryExpression.Left, true, binaryExpression.NodeType, enumType);
            _statement.Append($" {GetSqlOperator(binaryExpression.NodeType)} ");
            enumType = GetEnumType(binaryExpression.Left);
            ParseExpression(binaryExpression.Right, true, binaryExpression.NodeType, enumType);

            if (needsParentheses) _statement.Append(")");
        }
        else if (expression is ConstantExpression constantExpression)
        {
            AppendConstantValue(constantExpression.Value, enumType: enumType);
        }
        else if (expression is MethodCallExpression methodCallExpression)
        {
            ParseMethodCallExpression(methodCallExpression);
        }
        else if (expression is MemberExpression memberExpr && (memberExpr.Expression is null || memberExpr.Expression is ConstantExpression))
        {
            var capturedValue = GetValueFromCapturedVariable(memberExpr);
            AppendConstantValue(capturedValue);
        }
        else if (expression is UnaryExpression unaryExpr && unaryExpr.NodeType is ExpressionType.Convert)
        {
            ParseExpression(unaryExpr.Operand);
        }
        else if (expression is UnaryExpression unaryExpression)
        {
            _statement.Append(GetSqlOperator(unaryExpression.NodeType));
            _statement.Append(" ");

            if (unaryExpression.Operand is MethodCallExpression)
            {
                ParseExpression(unaryExpression.Operand);
            }
            else
            {
                _statement.Append("(");
                ParseExpression(unaryExpression.Operand);
                _statement.Append(")");
            }
        }
        else if (expression is MemberExpression memberExpression)
        {
            _statement.Append(memberExpression.Member.Name);
        }
        else
        {
            throw new NotSupportedException($"Expression type {expression.GetType().Name} is not supported");
        }
    }

    private void ParseMethodCallExpression(MethodCallExpression methodCallExpression)
    {
        if (methodCallExpression.Method.DeclaringType == typeof(string) && methodCallExpression.Method.Name == "StartsWith")
        {
            var attribute = methodCallExpression.Object as MemberExpression;
            Thrower.ThrowIfNull<MethodCallObjectNullException>(attribute, "Method call object is not a MemberExpression.");
            var value = GetConstantValue(methodCallExpression.Arguments.Single());
            var beginsWith = string.Format("begins_with({0}, ?)", attribute.Member.Name);
            _statement.Append(beginsWith);
            AppendConstantValue(value, false);
        }
        else if (methodCallExpression.Method.DeclaringType == typeof(string) && methodCallExpression.Method.Name == "Contains")
        {
            var attribute = methodCallExpression.Object as MemberExpression;
            Thrower.ThrowIfNull<MethodCallObjectNullException>(attribute, "Method call object is not a MemberExpression.");
            var value = GetConstantValue(methodCallExpression.Arguments.Single());
            var contains = string.Format("contains({0}, ?)", attribute.Member.Name);
            _statement.Append(contains);
            AppendConstantValue(value, false);
        }
        else if (methodCallExpression.Method.DeclaringType == typeof(Guid) && methodCallExpression.Method.Name == "Parse")
        {
            var value = GetConstantValue(methodCallExpression.Arguments.Single());
            _statement.Append(QuestionMark);
            AppendConstantValue(value, false);
        }
        else if (methodCallExpression.Method.DeclaringType == typeof(InExtensions) && methodCallExpression.Method.Name == "In")
        {
            var attributeMemberExpression = methodCallExpression.Arguments[0] as MemberExpression;
            Thrower.ThrowIfNull<MethodCallArgumentNullException>(attributeMemberExpression, "Method call argument is not a MemberExpression.");
            var attributeName = attributeMemberExpression.Member.Name;

            var values = (methodCallExpression.Arguments[1] as NewArrayExpression)?.Expressions.ToList();
            Thrower.ThrowIfNull<MethodCallArgumentNullException>(values, "Method call argument is not a NewArrayExpression.");
            values.ForEach(e =>
            {
                var value = GetConstantValue(e);
                AppendConstantValue(value, false);
            });
            _statement.Append(attributeName);
            _statement.Append(" IN (");
            for (int i = 0; i < values.Count; i++)
            {
                _statement.Append(QuestionMark);
                if (i < values.Count - 1)
                {
                    _statement.Append(", ");
                }
            }
            _statement.Append(")");

        }
        else if (methodCallExpression.Method.DeclaringType == typeof(BetweenExtensions) && methodCallExpression.Method.Name == "Between")
        {
            var attributeMemberExpression = methodCallExpression.Arguments[0] as MemberExpression;
            Thrower.ThrowIfNull<MethodCallArgumentNullException>(attributeMemberExpression, "Method call argument is not a MemberExpression.");
            var attributeName = attributeMemberExpression.Member.Name;
            var b = GetConstantValue(methodCallExpression.Arguments[1]);
            var c = GetConstantValue(methodCallExpression.Arguments[2]);
            _statement.Append(attributeName);
            _statement.Append(" BETWEEN ? AND ?");
            AppendConstantValue(b, addQuestionMark: false);
            AppendConstantValue(c, addQuestionMark: false);
        }
        else if (methodCallExpression.Object?.Type.GetInterfaces().Contains(typeof(IList)) == true && methodCallExpression.Method.Name == "Contains")
        {
            var attributeMemberExpression = methodCallExpression.Arguments.Single() as MemberExpression;
            Thrower.ThrowIfNull<MethodCallArgumentNullException>(attributeMemberExpression, "Method call argument is not a MemberExpression.");
            var attributeName = attributeMemberExpression.Member.Name;
            _statement.Append(attributeName);
            var memberExpression = methodCallExpression.Object as MemberExpression;
            Thrower.ThrowIfNull<MethodCallObjectNullException>(memberExpression, "Method call object is not a MemberExpression.");
            var closure = ((ConstantExpression?)memberExpression.Expression)?.Value;
            Thrower.ThrowIfNull<MemberExpressionClosureNullException>(closure, "Member expression closure is null.");
            var fieldInfo = (FieldInfo)memberExpression.Member;
            var list = (IList?)fieldInfo.GetValue(closure);

            _statement.Append(" IN [");
            for (int i = 0; i < list?.Count; i++)
            {
                AppendConstantValue(list[i]);

                if (i < list.Count - 1)
                {
                    _statement.Append(", ");
                }
            }
            _statement.Append("]");
        }
        else
        {
            throw new NotSupportedException($"Método {methodCallExpression.Method.Name} no es soportado");
        }
    }

    private static Type? GetEnumType(Expression expression)
    {
        if (expression is UnaryExpression unaryExpressionRight && unaryExpressionRight.Operand is MemberExpression memberOperandRight && memberOperandRight.Type.IsEnum)
        {
            return memberOperandRight.Type;
        }

        return null;
    }

    private void AppendConstantValue(object? value, bool addQuestionMark = true, Type? enumType = null)
    {
        Thrower.ThrowIfNull<ConstantNullException>(value, "Constant is null.");
        var stringValue = value.ToString();
        Thrower.ThrowIfNull<ConstantValueNullException>(stringValue, "Constant value string is null.");

        if (value is string strValue)
        {
            _parameters.Add(new StringAttributeValue(strValue));
        }
        else if (value is Guid guidValue)
        {
            _parameters.Add(new StringAttributeValue(guidValue.ToString()));
        }
        else if (value is DateTime dateTimeValue)
        {
            _parameters.Add(new StringAttributeValue(dateTimeValue.ToString("o")));
        }
        else if (enumType is not null && enumType.IsEnum)
        {
            object enumValue = Enum.Parse(enumType, stringValue);
            var enumValueString = enumValue.ToString();
            Thrower.ThrowIfNull<EnumValueNullException>(enumValueString, "Enum value is null.");
            _parameters.Add(new StringAttributeValue(enumValueString));
        }
        else
        {
            _parameters.Add(new NumberAttributeValue(stringValue));
        }

        if (addQuestionMark) _statement.Append(QuestionMark);
    }

    private static object? GetConstantValue(object value)
    {
        return value switch
        {
            string strValue => strValue,
            Guid guidValue => guidValue,
            DateTime dateTimeValue => dateTimeValue.ToString("o"),
            ConstantExpression constantExpression => constantExpression.Value,
            _ => value
        };
    }

    private static object GetValueFromCapturedVariable(MemberExpression memberExpression)
    {
        var objectMember = Expression.Convert(memberExpression, typeof(object));
        var getterLambda = Expression.Lambda<Func<object>>(objectMember);
        var getter = getterLambda.Compile();
        return getter();
    }

    private static string GetSqlOperator(ExpressionType expressionType)
    {
        return expressionType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "<>",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.OrElse => "OR",
            ExpressionType.Not => "NOT",
            ExpressionType.Convert => "",
            _ => throw new NotSupportedException($"Operator {expressionType} is not supported"),
        };
    }
}
