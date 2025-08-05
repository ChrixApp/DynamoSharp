using EfficientDynamoDb.DocumentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using DynamoSharp.Converters.Jsons;
using DynamoSharp.DynamoDb;

namespace DynamoSharp.Converters.Objects;

public sealed class ObjectConverter
{
    private static readonly Lazy<ObjectConverter> _instance = new Lazy<ObjectConverter>(() => new ObjectConverter());

    private ObjectConverter() { }

    public static ObjectConverter Instance => _instance.Value;

    public static AttributeValue CreateAttributeValue(object value)
    {
        return value switch
        {
            string s => new AttributeValue(new StringAttributeValue(s)),
            long l => new AttributeValue(new NumberAttributeValue(l.ToString())),
            double d => new AttributeValue(new NumberAttributeValue(d.ToString())),
            bool b => new AttributeValue(new BoolAttributeValue(b)),
            DateTime dt => new AttributeValue(new StringAttributeValue(dt.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK"))), // ISO 8601 format
            Guid g => new AttributeValue(new StringAttributeValue(g.ToString())),
            _ => throw new InvalidOperationException($"Unsupported value type: {value.GetType()}")
        };
    }

    public static List<Document> CloneList(IReadOnlyList<Document> documentsEfficient)
    {
        var documentsCopied = new List<Document>();
        for (var i = 0; i < documentsEfficient.Count; i++)
        {
            documentsCopied.Add(documentsEfficient[i]);
        }
        return documentsCopied;
    }

    public object DeepCopy(object rootEntity, JsonSerializer jsonSerializer)
    {
        var rootEntityJson = JObject.FromObject(rootEntity, jsonSerializer);
        var rootEntityJObject = JObjectToDynamoDbConverter.Instance.ConvertJObjectToDocument(rootEntityJson);
        return ConvertDocumentToObject(rootEntityJObject, rootEntity.GetType());
    }

    public object ConvertDocumentToObject(Document document, Type targetType)
    {
        var targetObject = RuntimeHelpers.GetUninitializedObject(targetType);
        var properties = DynamoSharpContext.EntityPropertiesCache.GetOrAdd(targetType, t => t.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));

        Parallel.ForEach(properties, propertyInfo =>
        {
            if (document.ContainsKey(propertyInfo.Name))
            {
                var attributeValue = document[propertyInfo.Name];

                if (attributeValue.IsNull || propertyInfo.PropertyType.IsClass && attributeValue.Type == AttributeType.Bool) return;

                var convertedValue = ConvertAttributeValueToType(attributeValue, propertyInfo.PropertyType);
                ReflectionUtils.SetValue(targetObject, targetType, propertyInfo.Name, convertedValue);
            }
            else if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var list = ConvertToList(new List<AttributeValue>(), propertyInfo.PropertyType);
                ReflectionUtils.SetValue(targetObject, targetType, propertyInfo.Name, list);
            }
            else if (propertyInfo.PropertyType.IsGenericType &&
                (propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>) ||
                propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>)))
            {
                var readOnlyList = ConvertToIReadOnlyList(new List<AttributeValue>(), propertyInfo.PropertyType);
                ReflectionUtils.SetValue(targetObject, targetType, propertyInfo.Name, readOnlyList);
            }
        });

        return targetObject;
    }

    private object? ConvertAttributeValueToType(AttributeValue attributeValue, Type propertyType)
    {
        return propertyType switch
        {
            _ when propertyType == typeof(string) => attributeValue.AsString(),
            _ when propertyType == typeof(short) || propertyType == typeof(short?) => short.Parse(attributeValue.AsNumberAttribute().Value),
            _ when propertyType == typeof(ushort) || propertyType == typeof(ushort?) => ushort.Parse(attributeValue.AsNumberAttribute().Value),
            _ when propertyType == typeof(int) || propertyType == typeof(int?) => int.Parse(attributeValue.AsNumberAttribute().Value),
            _ when propertyType == typeof(uint) || propertyType == typeof(uint?) => uint.Parse(attributeValue.AsNumberAttribute().Value),
            _ when propertyType == typeof(long) || propertyType == typeof(long?) => long.Parse(attributeValue.AsNumberAttribute().Value),
            _ when propertyType == typeof(ulong) || propertyType == typeof(ulong?) => ulong.Parse(attributeValue.AsNumberAttribute().Value),
            _ when propertyType == typeof(decimal) || propertyType == typeof(decimal?) => decimal.Parse(attributeValue.AsNumberAttribute().Value),
            _ when propertyType == typeof(double) || propertyType == typeof(double?) => double.Parse(attributeValue.AsNumberAttribute().Value),
            _ when propertyType == typeof(float) || propertyType == typeof(float?) => float.Parse(attributeValue.AsNumberAttribute().Value),
            _ when propertyType == typeof(byte) || propertyType == typeof(byte?) => byte.Parse(attributeValue.AsNumberAttribute().Value),
            _ when propertyType == typeof(bool) || propertyType == typeof(bool?) => attributeValue.AsBool(),
            _ when propertyType == typeof(char) || propertyType == typeof(char?) => char.Parse(attributeValue.AsString()),
            _ when propertyType == typeof(DateTime) || propertyType == typeof(DateTime?) => ConvertAttributeValueToDateTime(attributeValue),
            _ when propertyType.IsEnum => Enum.Parse(propertyType, attributeValue.AsString()),
            _ when propertyType == typeof(byte[]) => attributeValue.AsBinaryAttribute(),
            _ when propertyType == typeof(Guid) || propertyType == typeof(Guid?) => Guid.Parse(attributeValue.AsString()),
            _ when propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>) => ConvertToDictionary(attributeValue.AsDocument(), propertyType),
            _ when propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>) => ConvertToIReadOnlyDictionary(attributeValue.AsDocument(), propertyType),
            _ when propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>) => ConvertToList(attributeValue.AsListAttribute().Items, propertyType),
            _ when propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>) => ConvertToIReadOnlyList(attributeValue.AsListAttribute().Items, propertyType),
            _ when IsSmartEnum(propertyType) => ConvertToSmartEnum(propertyType, attributeValue),
            _ when propertyType.IsClass || propertyType.IsValueType => ConvertDocumentToObject(attributeValue.AsDocument(), propertyType),
            _ => throw new InvalidOperationException($"Cannot convert AttributeValue to {propertyType.Name}.")
        };
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

    private static object? ConvertToSmartEnum(Type smartEnumType, AttributeValue attributeValue)
    {
        var baseType = FindSmarEnumBaseType(smartEnumType);
        var method = baseType.GetMethod("FromName", BindingFlags.Static | BindingFlags.Public);

        if (method != null)
        {
            return method.Invoke(null, new object[] { attributeValue.AsString(), true });
        }

        return null;
    }

    private static Type FindSmarEnumBaseType(Type? type)
    {
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition().Name.Contains("SmartEnum"))
            {
                return type;
            }
            type = type.BaseType;
        }
        throw new InvalidOperationException("The type is not a SmartEnum.");
    }

    private static DateTime ConvertAttributeValueToDateTime(AttributeValue attributeValue)
    {
        if (attributeValue.IsNull)
            throw new InvalidOperationException("The AttributeValue is not a valid string.");

        var dateString = attributeValue.AsString();
        return DateTime.ParseExact(dateString, "yyyy-MM-ddTHH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    }

    private object? ConvertToDictionary(Document document, Type dictionaryType)
    {
        var keyType = dictionaryType.GetGenericArguments()[0];
        var valueType = dictionaryType.GetGenericArguments()[1];
        var dictionary = (IDictionary?)Activator.CreateInstance(dictionaryType);

        return FillDictionary(document, keyType, valueType, dictionary);
    }

    private object? ConvertToIReadOnlyDictionary(Document document, Type dictionaryType)
    {
        var keyType = dictionaryType.GetGenericArguments()[0];
        var valueType = dictionaryType.GetGenericArguments()[1];
        Type genericClassType = typeof(Dictionary<,>);
        Type constructedClassType = genericClassType.MakeGenericType(keyType, valueType);
        var dictionary = (IDictionary?)Activator.CreateInstance(constructedClassType);

        return FillDictionary(document, keyType, valueType, dictionary);
    }

    private IDictionary? FillDictionary(Document document, Type keyType, Type valueType, IDictionary? dictionary)
    {
        foreach (var kvp in document)
        {
            object? key;
            object? value;

            if (keyType == typeof(string))
            {
                key = kvp.Key;
            }
            else if (IsUserDefinedReferenceType(keyType) || keyType == typeof(Guid))
            {
                key = ConvertAttributeValueToType(kvp.Key, keyType);
            }
            else
            {
                key = Convert.ChangeType(kvp.Key, keyType);
            }

            if (valueType == typeof(string))
            {
                value = kvp.Value.AsString();
            }
            else if (IsUserDefinedReferenceType(valueType) || valueType == typeof(Guid))
            {
                value = ConvertAttributeValueToType(kvp.Value, valueType);
            }
            else
            {
                value = Convert.ChangeType(kvp.Value, valueType);
            }

            ArgumentNullException.ThrowIfNull(key);
            dictionary?.Add(key, value);
        }

        return dictionary;
    }

    private object? ConvertToList(List<AttributeValue> list, Type listType)
    {
        var elementType = listType.GetGenericArguments()[0];
        var genericList = (IList?)Activator.CreateInstance(listType);

        foreach (var item in list)
        {
            var convertedItem = ConvertAttributeValueToType(item, elementType);
            genericList?.Add(convertedItem);
        }

        return genericList;
    }

    private object? ConvertToIReadOnlyList(List<AttributeValue> list, Type listType)
    {
        var elementType = listType.GetGenericArguments()[0];
        Type genericClassType = typeof(List<>);
        Type constructedClassType = genericClassType.MakeGenericType(elementType);
        var genericList = (IList?)Activator.CreateInstance(constructedClassType);

        foreach (var item in list)
        {
            var convertedItem = ConvertAttributeValueToType(item, elementType);
            genericList?.Add(convertedItem);
        }

        return genericList;
    }

    private static bool IsUserDefinedReferenceType(Type type)
    {
        return (type.IsClass || type.IsInterface || type.IsEnum) &&
            !type.IsPrimitive &&
            type != typeof(string) &&
            !type.IsArray &&
            type.Namespace != null && !type.Namespace.StartsWith("System");
    }
}