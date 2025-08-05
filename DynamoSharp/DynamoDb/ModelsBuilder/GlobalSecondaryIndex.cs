namespace DynamoSharp.DynamoDb.ModelsBuilder;

public class GlobalSecondaryIndex
{
    public string Path { get; private set; }
    public string Prefix { get; private set; }
    public string Value { get; private set; }

    public GlobalSecondaryIndex(string path, string prefix)
    {
        Path = path;
        Prefix = prefix;
        Value = string.Empty;
    }

    public GlobalSecondaryIndex(string value)
    {
        Path = string.Empty;
        Prefix = string.Empty;
        Value = value;
    }

    public bool HasPath() => Path != string.Empty;
}
