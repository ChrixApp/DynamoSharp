using EfficientDynamoDb.DocumentModel;

namespace DynamoSharp.Tests.DynamoDb.QueryBuilder;

public static class QueryBuilderTestDataFactory
{
    private static Document CreateOrderDocument(string partitionKey, string sortKey, string id, string buyerId, string dateString)
    {
        return new Document
        {
            ["PartitionKey"] = partitionKey,
            ["SortKey"] = sortKey,
            ["Id"] = id,
            ["BuyerId"] = buyerId,
            ["Address"] = new Document
            {
                ["Street"] = "Street 1",
                ["City"] = "City 1",
                ["State"] = "State 1",
                ["ZipCode"] = "ZipCode 1"
            },
            ["Date"] = dateString,
        };
    }

    public static List<Document> CreateOrderDocuments()
    {
        var dateString = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK");
        return new List<Document>
        {
            CreateOrderDocument("ORDER#85cafc37-e6bb-4693-9283-f2eaec9828af", "ORDER#85cafc37-e6bb-4693-9283-f2eaec9828af", "85cafc37-e6bb-4693-9283-f2eaec9828af", "68139DA0-A9F5-42FB-97FA-0585E9BCC8B1", dateString),
            new Document
            {
                ["PartitionKey"] = "ORDER#85cafc37-e6bb-4693-9283-f2eaec9828af",
                ["SortKey"] = "ITEM#3DD8F3EE-6445-4D2F-BEEE-2BED65C17ECD",
                ["Id"] = "3DD8F3EE-6445-4D2F-BEEE-2BED65C17ECD",
                ["ProductName"] = "Product 1",
                ["UnitPrice"] = new NumberAttributeValue("10.99"),
                ["Units"] = new NumberAttributeValue("1")
            },
        };
    }

    public static List<Document> CreateActorDocuments()
    {
        return new List<Document>
        {
            new Document
            {
                ["MovieId"] = "dfcf58ec-0127-41df-b10c-9c3da1ce6da5",
                ["ActorId"] = "7d06c835-0ddc-4866-b42b-525221ded86c",
                ["MovieTitle"] = "3DD8F3EE-6445-4D2F-BEEE-2BED65C17ECD",
                ["ActorName"] = "The Matrix",
                ["RoleName"] = "Keanu Reeves",
                ["PartitionKey"] = "MOVIE#dfcf58ec-0127-41df-b10c-9c3da1ce6da5",
                ["SortKey"] = "ACTOR#7d06c835-0ddc-4866-b42b-525221ded86c",
                ["GSI1PK"] = "ACTOR#7d06c835-0ddc-4866-b42b-525221ded86c",
                ["GSI1SK"] = "MOVIE#dfcf58ec-0127-41df-b10c-9c3da1ce6da5"
            },
            new Document
            {
                ["MovieId"] = "d0316fa4-ac1b-49d4-8404-41158503721c",
                ["ActorId"] = "7d06c835-0ddc-4866-b42b-525221ded86c",
                ["MovieTitle"] = "John Wick",
                ["ActorName"] = "Keanu Reeves",
                ["RoleName"] = "John Wick",
                ["PartitionKey"] = "MOVIE#d0316fa4-ac1b-49d4-8404-41158503721c",
                ["SortKey"] = "ACTOR#7d06c835-0ddc-4866-b42b-525221ded86c",
                ["GSI1PK"] = "ACTOR#7d06c835-0ddc-4866-b42b-525221ded86c",
                ["GSI1SK"] = "MOVIE#d0316fa4-ac1b-49d4-8404-41158503721c"
            },
            new Document
            {
                ["Id"] = "7d06c835-0ddc-4866-b42b-525221ded86c",
                ["name"] = "Keanu Reeves",
                ["BirthDate"] = "1964-09-02T00:00:00",
                ["PartitionKey"] = "ACTOR#7d06c835-0ddc-4866-b42b-525221ded86c",
                ["SortKey"] = "ACTOR#7d06c835-0ddc-4866-b42b-525221ded86c",
                ["GSI1PK"] = "ACTOR#7d06c835-0ddc-4866-b42b-525221ded86c",
                ["GSI1SK"] = "ACTOR#7d06c835-0ddc-4866-b42b-525221ded86c"
            }
        };
    }

    public static List<Document> CreateMovieDocuments()
    {
        return new List<Document>
        {
            new Document
            {
                ["MovieId"] = "dfcf58ec-0127-41df-b10c-9c3da1ce6da5",
                ["ActorId"] = "7d06c835-0ddc-4866-b42b-525221ded86c",
                ["MovieTitle"] = "3DD8F3EE-6445-4D2F-BEEE-2BED65C17ECD",
                ["ActorName"] = "The Matrix",
                ["RoleName"] = "Keanu Reeves",
                ["PartitionKey"] = "MOVIE#dfcf58ec-0127-41df-b10c-9c3da1ce6da5",
                ["SortKey"] = "ACTOR#7d06c835-0ddc-4866-b42b-525221ded86c",
                ["GSI1PK"] = "ACTOR#7d06c835-0ddc-4866-b42b-525221ded86c",
                ["GSI1SK"] = "MOVIE#dfcf58ec-0127-41df-b10c-9c3da1ce6da5"
            },
            new Document
            {
                ["MovieId"] = "dfcf58ec-0127-41df-b10c-9c3da1ce6da5",
                ["ActorId"] = "de85699d-0c28-4775-be9b-9210581bec45",
                ["MovieTitle"] = "The Matrix",
                ["ActorName"] = "Carrie-Anne Moss",
                ["RoleName"] = "Trinity",
                ["PartitionKey"] = "MOVIE#dfcf58ec-0127-41df-b10c-9c3da1ce6da5",
                ["SortKey"] = "ACTOR#de85699d-0c28-4775-be9b-9210581bec45",
                ["GSI1PK"] = "ACTOR#de85699d-0c28-4775-be9b-9210581bec45",
                ["GSI1SK"] = "MOVIE#dfcf58ec-0127-41df-b10c-9c3da1ce6da5"
            },
            new Document
            {
                ["Id"] = "dfcf58ec-0127-41df-b10c-9c3da1ce6da5",
                ["Title"] = "The Matrix",
                ["Year"] = new NumberAttributeValue("1999"),
                ["Genre"] = "Science Fiction",
                ["IMDBScore"] = new NumberAttributeValue("8.7"),
                ["PartitionKey"] = "MOVIE#dfcf58ec-0127-41df-b10c-9c3da1ce6da5",
                ["SortKey"] = "MOVIE#dfcf58ec-0127-41df-b10c-9c3da1ce6da5"
            }
        };
    }

    public static List<Document> CreateAffiliationDocuments()
    {
        var dateString = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK");
        return new List<Document>
        {
            new Document
            {
                ["PartitionKey"] = "MERCHANT#68139DA0-A9F5-42FB-97FA-0585E9BCC8B1",
                ["SortKey"] = "Default#Other#MX#Default#Default",
                ["Id"] = "85cafc37-e6bb-4693-9283-f2eaec9828af",
                ["MerchantId"] = "68139DA0-A9F5-42FB-97FA-0585E9BCC8B1",
                ["TerminalId"] = "9F9CE712-D241-4D06-9D12-4A3D470E8682",
                ["Section"] = "Default",
                ["CardBrand"] = "Other",
                ["CountryOrRigion"] = "MX",
                ["Bank"] = "Default",
                ["AffiliationType"] = "Default",
                ["Percentage"] = new NumberAttributeValue("10.99"),
                ["CreatedAt"] = dateString
            }
        };
    }

    public static List<Document> CreateOrderDocumentsForListTest()
    {
        var dateString = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK");
        return new List<Document>
        {
            CreateOrderDocument("ORDER#85cafc37-e6bb-4693-9283-f2eaec9828af", "ORDER#85cafc37-e6bb-4693-9283-f2eaec9828af", "85cafc37-e6bb-4693-9283-f2eaec9828af", "68139DA0-A9F5-42FB-97FA-0585E9BCC8B1", dateString),
            CreateOrderDocument("ORDER#85cafc37-e6bb-4693-9283-f2eaec9828af", "ORDER#85cafc37-e6bb-4693-9283-f2eaec9828af", "85cafc37-e6bb-4693-9283-f2eaec9828af", "68139DA0-A9F5-42FB-97FA-0585E9BCC8B1", dateString)
        };
    }
}
