using DynamoSharp.Converters.Entities;
using DynamoSharp.DynamoDb.Configs;
using EfficientDynamoDb.DocumentModel;
using Newtonsoft.Json.Linq;

namespace DynamoSharp.Tests.Converters.Entities;

public static class EntityConverterTestDataFactory
{
    public static TableSchema CreateTableSchema()
    {
        return new TableSchema.TableSchemaBuilder()
            .WithTableName("TableName")
            .Build();
    }

    public static EntityConverter CreateEntityConverter() => new EntityConverter(CreateTableSchema());

    public static List<JObject> CreateEntitiesJson() => new List<JObject>
    {
        new JObject
        {
            { "Id", 1 },
            { "Name", "John" }
        },
        new JObject
        {
            { "Id", 2 },
            { "Name", "Jane" }
        }
    };

    public static List<Document> CreateDocuments() => new List<Document>
    {
        new Document
        {
            { "PartitionKey", "ORDER#23565964-51e5-4530-b289-05aedc8aae72" },
            { "SortKey", "ORDER#23565964-51e5-4530-b289-05aedc8aae72" }
        },
        new Document
        {
            { "PartitionKey", "ORDER#53c942ac-ef3c-40cf-ba17-d31a5d1ea1fe" },
            { "SortKey", "ORDER#53c942ac-ef3c-40cf-ba17-d31a5d1ea1fe" }
        }
    };

    public static List<Document> CreateDocumentsWithAttributes() => new List<Document>
    {
        new Document
        {
            { "PartitionKey", "ORDER#23565964-51e5-4530-b289-05aedc8aae72" },
            { "SortKey", "ORDER#23565964-51e5-4530-b289-05aedc8aae72" },
            { "Name", "John" },
            { "Age", 30 }
        },
        new Document
        {
            { "PartitionKey", "ORDER#53c942ac-ef3c-40cf-ba17-d31a5d1ea1fe" },
            { "SortKey", "ORDER#53c942ac-ef3c-40cf-ba17-d31a5d1ea1fe" },
            { "Name", "Jane" },
            { "Age", 25 }
        }
    };
}

