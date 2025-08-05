using System.Reflection;

namespace DynamoSharp.Utils;

public class NestedReferenceComparison : IComparison
{
    public bool Compare(object? obj1, object? obj2, HashSet<(object, object)> visited)
    {
        if (obj1 == null || obj2 == null)
        {
            return obj1 == obj2;
        }

        if (obj1.GetType() != obj2.GetType())
        {
            return false;
        }

        var pair = (obj1, obj2);
        if (visited.Contains(pair))
        {
            return true;
        }

        visited.Add(pair);

        var type = obj1.GetType();
        if (type.IsPrimitive || type == typeof(string))
        {
            return ReferenceEquals(obj1, obj2);
        }

        var result = false;
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        Parallel.ForEach(properties, (property, state) =>
        {
            var value1 = property.GetValue(obj1);
            var value2 = property.GetValue(obj2);

            if (value1 == null || value2 == null) return;

            if (ReferenceEquals(value1, value2) && Compare(value1, value2, visited))
            {
                result = true;
                state.Stop();
            }
        });

        return result;
    }
}
