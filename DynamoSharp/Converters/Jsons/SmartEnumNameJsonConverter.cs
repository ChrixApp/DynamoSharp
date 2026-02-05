using Newtonsoft.Json;
using System.Reflection;

namespace DynamoSharp.Converters.Jsons;

public sealed class SmartEnumNameJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
        => IsSmartEnum(objectType);

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        var nameProp = value.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
        var name = (string?)nameProp?.GetValue(value);

        writer.WriteValue(name);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        if (reader.TokenType != JsonToken.String)
            throw new JsonSerializationException($"Expected string for {objectType.Name} but got {reader.TokenType}.");

        var name = (string)reader.Value!;

        var fromName = objectType.GetMethod(
            "FromName",
            BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy,
            binder: null,
            types: new[] { typeof(string), typeof(bool) },
            modifiers: null);

        if (fromName is null)
            throw new MissingMethodException(objectType.FullName, "FromName(string,bool)");

        return fromName.Invoke(null, new object[] { name, false });
    }

    private static bool IsSmartEnum(Type? type)
    {
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition().Name.Contains("SmartEnum"))
            {
                return true;
            }
            type = type.BaseType;
        }
        return false;
    }
}
