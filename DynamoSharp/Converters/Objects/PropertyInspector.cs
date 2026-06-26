using System.Reflection;

namespace DynamoSharp.Converters.Objects;

public static class PropertyInspector
{
    private static bool HasCompilerBackingField(PropertyInfo p) =>
        p.DeclaringType?.GetField($"<{p.Name}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance) != null;

    public static bool IsComputedProperty(PropertyInfo p) =>
        p.CanRead
        && p.GetMethod != null
        && !HasCompilerBackingField(p);

    public static bool IsCollectionProperty(PropertyInfo p)
    {
        return p.PropertyType.IsGenericType &&
            (p.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>) ||
            p.PropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>) ||
            p.PropertyType.GetGenericTypeDefinition() == typeof(List<>) ||
            p.PropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>) ||
            p.PropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>));
    }

}