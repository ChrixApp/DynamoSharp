namespace DynamoSharp.Utils;

public interface IComparison
{
    bool Compare(object? obj1, object? obj2, HashSet<(object, object)> visited);
}
