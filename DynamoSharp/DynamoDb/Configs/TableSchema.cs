namespace DynamoSharp.DynamoDb.Configs;

public class TableSchema
{
    public readonly string TableName;
    public readonly string PartitionKeyName;
    public readonly string SortKeyName;
    public readonly string VersionName;
    private readonly List<GlobalSecondaryIndexSchema> _globalSecondaryIndices = new();

    public IReadOnlyList<GlobalSecondaryIndexSchema> GlobalSecondaryIndices => _globalSecondaryIndices;

    private TableSchema(
        string tableName,
        string partitionKeyName,
        string sortKeyName,
        string versionName,
        List<GlobalSecondaryIndexSchema> globalSecondaryIndices)
    {
        TableName = tableName;
        PartitionKeyName = partitionKeyName;
        SortKeyName = sortKeyName;
        VersionName = versionName;
        _globalSecondaryIndices = globalSecondaryIndices ?? new List<GlobalSecondaryIndexSchema>();
    }

    public string GetGlobalSecondaryIndexPartitionKey(string indexName)
    {
        var globalSecondaryIndexSchema = GlobalSecondaryIndices.FirstOrDefault(x => x.IndexName == indexName);
        if (globalSecondaryIndexSchema is null) throw new ArgumentException($"Index {indexName} not found", nameof(indexName));
        return globalSecondaryIndexSchema.PartitionKeyName;
    }

    public string GetGlobalSecondaryIndexSortKey(string indexName)
    {
        var globalSecondaryIndexSchema = GlobalSecondaryIndices.FirstOrDefault(x => x.IndexName == indexName);
        if (globalSecondaryIndexSchema is null) throw new ArgumentException($"Index {indexName} not found", nameof(indexName));
        return globalSecondaryIndexSchema.SortKeyName;
    }

    public bool HasVersioning()
    {
        return !string.IsNullOrWhiteSpace(VersionName);
    }

    public class Builder
    {
        private string _tableName = string.Empty;
        private string _partitionKeyName = "PartitionKey";
        private string _sortKeyName = "SortKey";
        private string _versionName = string.Empty;
        private readonly List<GlobalSecondaryIndexSchema> _globalSecondaryIndices = new();

        public Builder WithTableName(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentNullException(nameof(tableName), "Null or empty not allowed");
            _tableName = tableName;
            return this;
        }

        public Builder WithPartitionKeyName(string partitionKeyName)
        {
            if (string.IsNullOrWhiteSpace(partitionKeyName)) throw new ArgumentNullException(nameof(partitionKeyName), "Null or empty not allowed");
            _partitionKeyName = partitionKeyName;
            return this;
        }

        public Builder WithSortKeyName(string sortKeyName)
        {
            if (string.IsNullOrWhiteSpace(sortKeyName)) throw new ArgumentNullException(nameof(sortKeyName), "Null or empty not allowed");
            _sortKeyName = sortKeyName;
            return this;
        }

        public Builder WithVersionName(string versionName)
        {
            _versionName = versionName;
            return this;
        }

        public Builder AddGlobalSecondaryIndex(params GlobalSecondaryIndexSchema[] globalSecondaryIndices)
        {
            if (globalSecondaryIndices is null) throw new ArgumentNullException(nameof(globalSecondaryIndices), "Null not allowed");
            if (_globalSecondaryIndices.Count + globalSecondaryIndices.Length > 20) throw new InvalidOperationException("Cannot add more than 20 Global Secondary Indices to a TableSchema.");
            foreach (var gsi in globalSecondaryIndices)
            {
                if (gsi is null) throw new ArgumentNullException(nameof(globalSecondaryIndices), "Null not allowed");
                _globalSecondaryIndices.Add(gsi);
            }
            return this;
        }

        public Builder AddGlobalSecondaryIndex(string indexName, string partitionKeyName, string sortKeyName)
        {
            if (_globalSecondaryIndices.Count > 20) throw new InvalidOperationException("Cannot add more than 20 Global Secondary Indices to a TableSchema.");
            var gsi = new GlobalSecondaryIndexSchema(indexName, partitionKeyName, sortKeyName);
            _globalSecondaryIndices.Add(gsi);
            return this;
        }

        public TableSchema Build()
        {
            if (string.IsNullOrWhiteSpace(_tableName)) throw new InvalidOperationException("TableName must be set before building the TableSchema.");
            return new TableSchema(_tableName, _partitionKeyName, _sortKeyName, _versionName, _globalSecondaryIndices);
        }
    }
}

public class GlobalSecondaryIndexSchema
{
    public string IndexName { get; private set; }
    public string PartitionKeyName { get; private set; }
    public string SortKeyName { get; private set; }

    public GlobalSecondaryIndexSchema(string indexName, string partitionKeyName, string sortKeyName)
    {
        if (string.IsNullOrWhiteSpace(indexName)) throw new ArgumentNullException(nameof(indexName), "Null or empty not allowed");
        if (string.IsNullOrWhiteSpace(partitionKeyName)) throw new ArgumentNullException(nameof(partitionKeyName), "Null or empty not allowed");
        if (string.IsNullOrWhiteSpace(sortKeyName)) throw new ArgumentNullException(nameof(sortKeyName), "Null or empty not allowed");

        IndexName = indexName;
        PartitionKeyName = partitionKeyName;
        SortKeyName = sortKeyName;
    }
}