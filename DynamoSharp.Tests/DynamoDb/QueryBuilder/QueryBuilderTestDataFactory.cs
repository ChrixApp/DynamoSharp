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
                ["SortKey"] = "ORDERITEM#3DD8F3EE-6445-4D2F-BEEE-2BED65C17ECD",
                ["Id"] = "3DD8F3EE-6445-4D2F-BEEE-2BED65C17ECD",
                ["ProductName"] = "Product 1",
                ["UnitPrice"] = new NumberAttributeValue("10.99"),
                ["Units"] = new NumberAttributeValue("1")
            },
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
