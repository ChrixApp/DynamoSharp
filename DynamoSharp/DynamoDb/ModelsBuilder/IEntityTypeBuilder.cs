namespace DynamoSharp.DynamoDb.ModelsBuilder;

public interface IEntityTypeBuilder
{
    string IdName { get; }
    IDictionary<string, string> PartitionKey { get; }
    IDictionary<string, string> SortKey { get; }
    IDictionary<string, IList<GlobalSecondaryIndex>> GlobalSecondaryIndexPartitionKey { get; }
    IDictionary<string, IList<GlobalSecondaryIndex>> GlobalSecondaryIndexSortKey { get; }
    IDictionary<string, Type> OneToMany { get; }
    IDictionary<string, Type> ManyToMany { get; }
    bool Versioning { get; }
}
