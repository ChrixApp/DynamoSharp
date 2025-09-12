using Newtonsoft.Json.Linq;

namespace DynamoSharp.DynamoDb.DynamoEntities;

public class JTokenConverter
{
    public static string ConvertToString(JToken? property)
    {
        var propertyValue = property?.ToString() ?? string.Empty;

        if (property?.Type == JTokenType.Date)
        {
            var date = property.ToObject<DateTime>();
            propertyValue = date.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK");
        }
        else if (property?.Type == JTokenType.Array)
        {
            var list = property.ToObject<List<object>>();
            propertyValue = string.Join("#", list!);
        }

        return propertyValue;
    }
}
