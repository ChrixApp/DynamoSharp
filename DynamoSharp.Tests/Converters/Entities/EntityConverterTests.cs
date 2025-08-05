using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DynamoSharp.Tests.Converters.Entities;

public class EntityConverterTests
{
    [Fact]
    public void JsonListToDocuments_ShouldReturnListOfDocuments()
    {
        // Arrange
        var entityConverter = EntityConverterTestDataFactory.CreateEntityConverter();
        var entitiesJson = EntityConverterTestDataFactory.CreateEntitiesJson();

        // Act
        var result = entityConverter.JsonListToDocuments(entitiesJson);

        // Assert
        result.Count.Should().Be(2);
        result[0].Count.Should().Be(2);
        result[0]["Id"].AsNumberAttribute().Value.Should().Be("1");
        result[0]["Name"].AsString().Should().Be("John");
        result[1].Count.Should().Be(2);
        result[1]["Id"].AsNumberAttribute().Value.Should().Be("2");
        result[1]["Name"].AsString().Should().Be("Jane");
    }

    [Fact]
    public void JsonListToDocuments_ShouldReturnEmptyList_WhenEntitiesJsonIsEmpty()
    {
        // Arrange
        var entityConverter = EntityConverterTestDataFactory.CreateEntityConverter();
        var entitiesJson = new List<JObject>();

        // Act
        var result = entityConverter.JsonListToDocuments(entitiesJson);

        // Assert
        result.Count.Should().Be(0);
    }

    [Fact]
    public void DocumentsToBatchWriteDeleteRequests_ShouldReturnListOfBatchWriteOperation()
    {
        // Arrange
        var entityConverter = EntityConverterTestDataFactory.CreateEntityConverter();
        var documents = EntityConverterTestDataFactory.CreateDocuments();

        // Act
        var result = entityConverter.DocumentsToBatchWriteDeleteRequests(documents);

        // Assert
        result.Count.Should().Be(2);
        result[0].DeleteRequest?.Key["PartitionKey"].AsString().Should().Be("ORDER#23565964-51e5-4530-b289-05aedc8aae72");
        result[0].DeleteRequest?.Key["SortKey"].AsString().Should().Be("ORDER#23565964-51e5-4530-b289-05aedc8aae72");
        result[1].DeleteRequest?.Key["PartitionKey"].AsString().Should().Be("ORDER#53c942ac-ef3c-40cf-ba17-d31a5d1ea1fe");
        result[1].DeleteRequest?.Key["SortKey"].AsString().Should().Be("ORDER#53c942ac-ef3c-40cf-ba17-d31a5d1ea1fe");
    }

    [Fact]
    public void DocumentsToBatchWritePutRequests_ShouldReturnListOfBatchWriteOperation()
    {
        // Arrange
        var entityConverter = EntityConverterTestDataFactory.CreateEntityConverter();
        var documents = EntityConverterTestDataFactory.CreateDocumentsWithAttributes();

        // Act
        var result = entityConverter.DocumentsToBatchWritePutRequests(documents);

        // Assert
        result.Count.Should().Be(2);
        result[0].PutRequest?.Item["PartitionKey"].AsString().Should().Be("ORDER#23565964-51e5-4530-b289-05aedc8aae72");
        result[0].PutRequest?.Item["SortKey"].AsString().Should().Be("ORDER#23565964-51e5-4530-b289-05aedc8aae72");
        result[0].PutRequest?.Item["Name"].AsString().Should().Be("John");
        result[0].PutRequest?.Item["Age"].AsNumberAttribute().ToInt().Should().Be(30);
        result[1].PutRequest?.Item["PartitionKey"].AsString().Should().Be("ORDER#53c942ac-ef3c-40cf-ba17-d31a5d1ea1fe");
        result[1].PutRequest?.Item["SortKey"].AsString().Should().Be("ORDER#53c942ac-ef3c-40cf-ba17-d31a5d1ea1fe");
        result[1].PutRequest?.Item["Name"].AsString().Should().Be("Jane");
        result[1].PutRequest?.Item["Age"].AsNumberAttribute().ToInt().Should().Be(25);
    }

    [Fact]
    public void DocumentsToTransactDeleteWriteItems_ShouldReturnListOfTransactWriteItem()
    {
        // Arrange
        var entityConverter = EntityConverterTestDataFactory.CreateEntityConverter();
        var documents = EntityConverterTestDataFactory.CreateDocuments();

        // Act
        var result = entityConverter.DocumentsToTransactDeleteWriteItems(documents);

        // Assert
        result.Count.Should().Be(2);
        result[0].Delete?.Key?.PartitionKeyValue.AsString().Should().Be("ORDER#23565964-51e5-4530-b289-05aedc8aae72");
        result[0].Delete?.Key?.SortKeyValue?.AsString().Should().Be("ORDER#23565964-51e5-4530-b289-05aedc8aae72");
        result[1].Delete?.Key?.PartitionKeyValue.AsString().Should().Be("ORDER#53c942ac-ef3c-40cf-ba17-d31a5d1ea1fe");
        result[1].Delete?.Key?.SortKeyValue?.AsString().Should().Be("ORDER#53c942ac-ef3c-40cf-ba17-d31a5d1ea1fe");
    }

    [Fact]
    public void DocumentsToTransactPutWriteItems_ShouldReturnListOfTransactWriteItem()
    {
        // Arrange
        var entityConverter = EntityConverterTestDataFactory.CreateEntityConverter();
        var documents = EntityConverterTestDataFactory.CreateDocumentsWithAttributes();

        // Act
        var result = entityConverter.DocumentsToTransactPutWriteItems(documents);

        // Assert
        result.Count.Should().Be(2);
        result[0].Put?.Item?["PartitionKey"].AsString().Should().Be("ORDER#23565964-51e5-4530-b289-05aedc8aae72");
        result[0].Put?.Item?["SortKey"].AsString().Should().Be("ORDER#23565964-51e5-4530-b289-05aedc8aae72");
        result[0].Put?.Item?["Name"].AsString().Should().Be("John");
        result[0].Put?.Item?["Age"].AsNumberAttribute().ToInt().Should().Be(30);
        result[1].Put?.Item?["PartitionKey"].AsString().Should().Be("ORDER#53c942ac-ef3c-40cf-ba17-d31a5d1ea1fe");
        result[1].Put?.Item?["SortKey"].AsString().Should().Be("ORDER#53c942ac-ef3c-40cf-ba17-d31a5d1ea1fe");
        result[1].Put?.Item?["Name"].AsString().Should().Be("Jane");
        result[1].Put?.Item?["Age"].AsNumberAttribute().ToInt().Should().Be(25);
    }

    [Fact]
    public void DocumentsToTransactUpdateWriteItems_ShouldReturnListOfTransactWriteItem()
    {
        // Arrange
        var entityConverter = EntityConverterTestDataFactory.CreateEntityConverter();
        var documents = EntityConverterTestDataFactory.CreateDocumentsWithAttributes();

        // Act
        var result = entityConverter.DocumentsToTransactUpdateWriteItems(documents);

        // Assert
        result.Count.Should().Be(2);
        result[0].Update?.Key?.PartitionKeyValue.AsString().Should().Be("ORDER#23565964-51e5-4530-b289-05aedc8aae72");
        result[0].Update?.Key?.SortKeyValue?.AsString().Should().Be("ORDER#23565964-51e5-4530-b289-05aedc8aae72");
        result[0].Update?.UpdateExpression.Should().Be("SET #Name = :Name, #Age = :Age");
        result[0].Update?.ExpressionAttributeNames?["#Name"].Should().Be("Name");
        result[0].Update?.ExpressionAttributeValues?[":Name"].AsString().Should().Be("John");
        result[0].Update?.ExpressionAttributeNames?["#Age"].Should().Be("Age");
        result[0].Update?.ExpressionAttributeValues?[":Age"].AsNumberAttribute().ToInt().Should().Be(30);
        result[1].Update?.Key?.PartitionKeyValue.AsString().Should().Be("ORDER#53c942ac-ef3c-40cf-ba17-d31a5d1ea1fe");
        result[1].Update?.Key?.SortKeyValue?.AsString().Should().Be("ORDER#53c942ac-ef3c-40cf-ba17-d31a5d1ea1fe");
        result[1].Update?.UpdateExpression.Should().Be("SET #Name = :Name, #Age = :Age");
        result[1].Update?.ExpressionAttributeNames?["#Name"].Should().Be("Name");
        result[1].Update?.ExpressionAttributeValues?[":Name"].AsString().Should().Be("Jane");
        result[1].Update?.ExpressionAttributeNames?["#Age"].Should().Be("Age");
        result[1].Update?.ExpressionAttributeValues?[":Age"].AsNumberAttribute().ToInt().Should().Be(25);
    }
}
