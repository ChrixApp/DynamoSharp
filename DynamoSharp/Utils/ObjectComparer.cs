namespace DynamoSharp.Utils;

public static class ObjectComparer
{
    private static readonly IComparison _comparisonStrategy = new NestedReferenceComparison();

    public static bool HaveSameNestedReferences(object? obj1, object? obj2)
    {
        return _comparisonStrategy.Compare(obj1, obj2, new HashSet<(object, object)>());
    }
}