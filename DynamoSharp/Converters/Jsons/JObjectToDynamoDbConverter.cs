using EfficientDynamoDb.DocumentModel;
using Newtonsoft.Json.Linq;

namespace DynamoSharp.Converters.Jsons;

public sealed class JObjectToDynamoDbConverter
{
    private static readonly Lazy<JObjectToDynamoDbConverter> _instance = new Lazy<JObjectToDynamoDbConverter>(() => new JObjectToDynamoDbConverter());

    private JObjectToDynamoDbConverter()
    {
        _strategies = new Dictionary<JTokenType, ITokenConverter>
        {
            { JTokenType.Object, new ObjectTokenConverter() },
            { JTokenType.Array, new ArrayTokenConverter() },
            { JTokenType.Integer, new PrimitiveTokenConverter() },
            { JTokenType.Float, new PrimitiveTokenConverter() },
            { JTokenType.String, new PrimitiveTokenConverter() },
            { JTokenType.Boolean, new PrimitiveTokenConverter() },
            { JTokenType.Null, new PrimitiveTokenConverter() },
            { JTokenType.Date, new PrimitiveTokenConverter() },
            { JTokenType.Guid, new PrimitiveTokenConverter() }
        };
    }

    public static JObjectToDynamoDbConverter Instance => _instance.Value;

    private readonly Dictionary<JTokenType, ITokenConverter> _strategies;

    public AttributeValue ConvertJTokenToAttributeValue(JToken token)
    {
        if (_strategies.TryGetValue(token.Type, out var strategy))
        {
            return strategy.Convert(token);
        }
        throw new InvalidOperationException($"Unsupported JTokenType: {token.Type}");
    }

    public Document ConvertJObjectToDocument(JObject jObject)
    {
        var document = new Document();

        foreach (var property in jObject.Properties())
        {
            document[property.Name] = ConvertJTokenToAttributeValue(property.Value);
        }

        return document;
    }
}
