using EfficientDynamoDb.DocumentModel;
using Newtonsoft.Json.Linq;

namespace DynamoSharp.Converters.Jsons;

public class ObjectTokenConverter : ITokenConverter
{
    public AttributeValue Convert(JToken token)
    {
        var map = new Document();
        foreach (var prop in ((JObject)token).Properties())
        {
            map[prop.Name] = JObjectToDynamoDbConverter.Instance.ConvertJTokenToAttributeValue(prop.Value);
        }
        return new AttributeValue(new MapAttributeValue(map));
    }
}
