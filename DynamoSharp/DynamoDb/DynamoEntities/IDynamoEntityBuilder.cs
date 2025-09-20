using DynamoSharp.ChangeTracking;
using Newtonsoft.Json.Linq;

namespace DynamoSharp.DynamoDb.DynamoEntities;

public interface IDynamoEntityBuilder
{
    JObject BuildAddedEntity(EntityChangeTracker entityEntry);
    JObject BuildDeletedEntity(EntityChangeTracker entityEntry);
    JObject BuildModifiedEntity(EntityChangeTracker entityEntry);
}