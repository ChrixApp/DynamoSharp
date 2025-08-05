using DynamoSharp.DynamoDb.ModelsBuilder;

namespace DynamoSharp.ChangeTracking;

public class EntityEqualityComparer : IEqualityComparer<object>
{
    private readonly IModelBuilder _modelBuilder;

    public EntityEqualityComparer(IModelBuilder modelBuilder)
    {
        _modelBuilder = modelBuilder;
    }

    public new bool Equals(object? x, object? y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);

        if (x.GetType() != y.GetType()) return false;

        var idName = _modelBuilder.Entities[x.GetType()].IdName ?? "Id";
        var xId = x.GetType().GetProperty(idName)?.GetValue(x);
        var yId = y.GetType().GetProperty(idName)?.GetValue(y);
        return xId?.Equals(yId) ?? x.GetHashCode() == y.GetHashCode();
    }

    public int GetHashCode(object obj)
    {
        var idName = _modelBuilder.Entities[obj.GetType()].IdName ?? "Id";
        var id = obj.GetType().GetProperty(idName)?.GetValue(obj);
        return id?.GetHashCode() ?? obj.GetHashCode();
    }
}
