using Newtonsoft.Json;
using System.Reflection;

namespace DynamoSharp.Converters.Jsons;

public sealed class SmartEnumNameJsonConverter : JsonConverter
{
    private readonly bool UseValueForSmartEnum;

    public SmartEnumNameJsonConverter(bool useValueForSmartEnum)
    {
        UseValueForSmartEnum = useValueForSmartEnum;
    }

    public SmartEnumNameJsonConverter()
    {
        UseValueForSmartEnum = true;
    }

    public override bool CanConvert(Type objectType)
        => IsSmartEnum(objectType);

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        var propertyName = UseValueForSmartEnum ? "Value" : "Name";

        var prop = value.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        var smartEnum = prop?.GetValue(value);

        writer.WriteValue(smartEnum);
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
