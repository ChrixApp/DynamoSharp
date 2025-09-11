using System.Reflection;

namespace DynamoSharp.Converters.Objects;

public static class PropertyInspector
{
    private static bool HasCompilerBackingField(Type type, PropertyInfo p) =>
        type.GetField($"<{p.Name}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance) != null;

    public static bool IsComputedProperty(Type type, PropertyInfo p) =>
        p.CanRead
        && p.GetMethod != null
        && !HasCompilerBackingField(type, p);

    public static bool IsCollectionProperty(Type type, PropertyInfo p)
    {
        return p.PropertyType.IsGenericType &&
            (p.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>) ||
            p.PropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>) ||
            p.PropertyType.GetGenericTypeDefinition() == typeof(List<>) ||
            p.PropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>) ||
            p.PropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>));
    }

}