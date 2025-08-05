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
                var value = t.Token!.Type == JTokenType.Date
                    ? t.Token.ToObject<DateTime>().ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK")
                    : t.Token.ToString();
                return string.IsNullOrWhiteSpace(t.Prefix) ? value : $"{t.Prefix}#{value}";
            })
            .Where(part => !string.IsNullOrWhiteSpace(part)));
    }
} 
