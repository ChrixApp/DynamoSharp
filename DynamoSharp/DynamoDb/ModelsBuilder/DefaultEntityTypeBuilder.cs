namespace DynamoSharp.DynamoDb.ModelsBuilder;

internal class DefaultEntityTypeBuilder : IEntityTypeBuilder
{
    public string IdName { get; } = "Id";
    public IDictionary<string, string> PartitionKey { get; private set; } = new Dictionary<string, string>();
    public IDictionary<string, string> SortKey { get; private set; } = new Dictionary<string, string>();
    public IDictionary<string, IList<GlobalSecondaryIndex>> GlobalSecondaryIndexPartitionKey { get; private set; } = new Dictionary<string, IList<GlobalSecondaryIndex>>();
    public IDictionary<string, IList<GlobalSecondaryIndex>> GlobalSecondaryIndexSortKey { get; private set; } = new Dictionary<string, IList<GlobalSecondaryIndex>>();

    public IDictionary<string, Type> OneToMany { get; private set; } = new Dictionary<string, Type>();
    public IDictionary<string, Type> ManyToMany { get; private set; } = new Dictionary<string, Type>();

    public IList<string> IgnoredProperties { get; private set; } = new List<string>();

    public bool Versioning { get; private set; } = false;

    public void SetPartitionKey(string name, string prefix)
    {
        PartitionKey.Add(name, prefix);
    }

    public void SetSortKey(string name, string prefix)
    {
        SortKey.Add(name, prefix);
    }
}
