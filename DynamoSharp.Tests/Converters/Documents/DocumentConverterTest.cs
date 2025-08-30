using DynamoSharp.Converters.Documents;
using DynamoSharp.Tests.Contexts.Models;
using EfficientDynamoDb.DocumentModel;
using FluentAssertions;
using System.Globalization;

namespace DynamoSharp.Tests.Converters.Documents;

public class DocumentConverterTest
{
    [Fact]
    public void CloneList_ShouldReturnClonedList()
    {
        // Arrange
        var documentsEfficient = new List<Document>
        {
            new Document(),
            new Document(),
            new Document()
        };

        var documentConverter = new DocumentConverter();

        // Act
        var result = documentConverter.CloneList(documentsEfficient.AsReadOnly());

        // Assert
        Assert.Equal(documentsEfficient.Count, result.Count);
        for (var i = 0; i < documentsEfficient.Count; i++)
        {
            Assert.Equal(documentsEfficient[i], result[i]);
        }
    }

    [Fact]
    public void ConvertToObject_ShouldReturnOrderObject()
    {
        // Arrange
        var dateString = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK");
        var doc = new Document
        {
            ["PartitionKey"] = "ORDER#85cafc37-e6bb-4693-9283-f2eaec9828af",
            ["SortKey"] = "ORDER#85cafc37-e6bb-4693-9283-f2eaec9828af",
            ["Id"] = "85cafc37-e6bb-4693-9283-f2eaec9828af",
            ["BuyerId"] = "68139DA0-A9F5-42FB-97FA-0585E9BCC8B1",
            ["Address"] = new Document
            {
                ["Street"] = "Street 1",
                ["City"] = "City 1",
                ["State"] = "State 1",
                ["ZipCode"] = "ZipCode 1"
            },
            ["Date"] = dateString,
        };

        var propertyType = typeof(Order);

        var documentConverter = new DocumentConverter();

        // Act
        var result = documentConverter.ConvertToObject(doc, propertyType);

        // Assert
        Assert.IsType<Order>(result);
        var entity = (Order)result;
        entity.Should().BeOfType<Order>();
        entity.Id.Should().Be("85cafc37-e6bb-4693-9283-f2eaec9828af");
        entity.BuyerId.Should().Be("68139DA0-A9F5-42FB-97FA-0585E9BCC8B1");
        entity.Date.Should().Be(DateTime.ParseExact(dateString, "yyyy-MM-ddTHH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
        entity.Address.Should().NotBeNull();
        entity.Address.Should().BeOfType<Address>();
        entity.Address?.Street.Should().Be("Street 1");
        entity.Address?.City.Should().Be("City 1");
        entity.Address?.State.Should().Be("State 1");
        entity.Address?.ZipCode.Should().Be("ZipCode 1");
        entity.Items.Should().HaveCount(0);
    }
}
