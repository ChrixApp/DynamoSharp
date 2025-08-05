using DynamoSharp.Exceptions;
using EfficientDynamoDb.DocumentModel;
using Newtonsoft.Json.Linq;

namespace DynamoSharp.Converters.Jsons;

public class PrimitiveTokenConverter : ITokenConverter
{
    public AttributeValue Convert(JToken token)
    {
        return token.Type switch
        {
            JTokenType.Integer => new AttributeValue(new NumberAttributeValue(token.Value<long>().ToString())),
            JTokenType.Float => new AttributeValue(new NumberAttributeValue(token.Value<double>().ToString())),
            JTokenType.String => CreateStringAttributeValue(token),
            JTokenType.Boolean => new AttributeValue(new BoolAttributeValue(token.Value<bool>())),
            JTokenType.Null => AttributeValue.Null,
            JTokenType.Date => new AttributeValue(new StringAttributeValue(token.Value<DateTime>().ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK"))), // ISO 8601 format
            JTokenType.Guid => new AttributeValue(new StringAttributeValue(token.Value<Guid>().ToString())),
            _ => throw new InvalidOperationException($"Unsupported JTokenType: {token.Type}")
        };
    }

    private static AttributeValue CreateStringAttributeValue(JToken token)
    {
        var stringToken = token.Value<string>();
        Thrower.ThrowIfNull<JTokenNullException>(stringToken);
        return new AttributeValue(new StringAttributeValue(stringToken));
    }
}
