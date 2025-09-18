using Newtonsoft.Json.Linq;

namespace DynamoSharp.DynamoDb.DynamoEntities;

public static class KeyBuilder
{
    public static string BuildKey(IDictionary<string, string> keyPaths, JObject jObject)
    {
        return string.Join("#", keyPaths
            .Select(kvp => (Prefix: kvp.Value, Token: jObject.SelectToken(kvp.Key)))
            .Where(t => t.Token != null)
            .Select(t =>
            {
                var value = JTokenConverter.ConvertToString(t.Token);
                return string.IsNullOrWhiteSpace(t.Prefix) ? value : $"{t.Prefix}#{value}";
            }));
    }
} 
