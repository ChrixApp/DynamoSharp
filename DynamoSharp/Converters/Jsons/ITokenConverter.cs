using EfficientDynamoDb.DocumentModel;
using Newtonsoft.Json.Linq;

namespace DynamoSharp.Converters.Jsons;

public interface ITokenConverter
{
    AttributeValue Convert(JToken token);
}
