using DynamoSharp.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DynamoSharp.Converters.Jsons;

public static class JsonSerializerBuilder
{
    public  static JsonSerializer Build(
        List<string> propertiesToIgnore,
        string dateFormatString = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK",
        int maxDepth = 10,
        NullValueHandling nullValueHandling = NullValueHandling.Ignore)
    {
        var jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CustomIgnoreResolver(propertiesToIgnore),
            Converters = new List<JsonConverter> { new StringEnumConverter() },
            DateFormatString = dateFormatString,
            MaxDepth = maxDepth,
            NullValueHandling = nullValueHandling,
        };
        return JsonSerializer.Create(jsonSerializerSettings);
    }

    public static JsonSerializer Build(
        string dateFormatString = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK",
        int maxDepth = 10,
        NullValueHandling nullValueHandling = NullValueHandling.Ignore)
    {
        var jsonSerializerSettings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new StringEnumConverter() },
            DateFormatString = dateFormatString,
            MaxDepth = maxDepth,
            NullValueHandling = nullValueHandling,
        };
        return JsonSerializer.Create(jsonSerializerSettings);
    }
}
