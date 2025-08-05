using EfficientDynamoDb.DocumentModel;
using Newtonsoft.Json.Linq;

namespace DynamoSharp.Converters.Jsons;

public class ArrayTokenConverter : ITokenConverter
{
    public AttributeValue Convert(JToken token)
    {
        var list = new List<AttributeValue>();
        foreach (var item in (JArray)token)
        {
            list.Add(JObjectToDynamoDbConverter.Instance.ConvertJTokenToAttributeValue(item));
        }
        return new AttributeValue(new ListAttributeValue(list));
    }
}
