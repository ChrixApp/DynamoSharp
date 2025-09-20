using Newtonsoft.Json.Linq;

namespace DynamoSharp.DynamoDb.DynamoEntities;

public class JTokenConverter
{
    public static object ConvertToString(JToken? property)
    {

        switch(property?.Type)
        {
            case JTokenType.String:
            case JTokenType.Guid:
                return property?.ToString() ?? string.Empty;
            case JTokenType.Date:
                var date = property.ToObject<DateTime>();
                return date.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK");
            case JTokenType.Integer:
                return property.ToObject<long>();
            case JTokenType.Float:
                return property.ToObject<decimal>();
            case JTokenType.Array:
                var list = property.ToObject<List<object>>();
                return string.Join("#", list!);
            default:
                return string.Empty;
        }
    }
}
