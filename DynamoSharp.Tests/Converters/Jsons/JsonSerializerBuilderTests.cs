using DynamoSharp.Converters.Jsons;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DynamoSharp.Tests.Converters.Jsons;

public class JsonSerializerBuilderTests
{
    [Fact]
    public void Build_WithoutPropertiesToIgnore_ReturnsDefaultSerializer()
    {
        // Arrange
        var dateFormatString = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK";
        var maxDepth = 10;
        var nullValueHandling = NullValueHandling.Ignore;

        // Act
        var serializer = JsonSerializerBuilder.Build(dateFormatString, maxDepth, nullValueHandling);

        // Assert
        Assert.NotNull(serializer);
        Assert.Equal(dateFormatString, serializer.DateFormatString);
        Assert.Equal(maxDepth, serializer.MaxDepth);
        Assert.Equal(nullValueHandling, serializer.NullValueHandling);
    }

    [Fact]
    public void Build_WithPropertiesToIgnore_ReturnsCustomSerializer()
    {
        // Arrange
        var propertiesToIgnore = new List<string> { "Property1", "Property2" };
        var dateFormatString = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK";
        var maxDepth = 10;
        var nullValueHandling = NullValueHandling.Ignore;

        // Act
        var serializer = JsonSerializerBuilder.Build(propertiesToIgnore, dateFormatString, maxDepth, nullValueHandling);

        // Assert
        Assert.NotNull(serializer);
        Assert.Equal(dateFormatString, serializer.DateFormatString);
        Assert.Equal(maxDepth, serializer.MaxDepth);
        Assert.Equal(nullValueHandling, serializer.NullValueHandling);
    }

    [Theory]
    [MemberData(nameof(JsonSerializerBuilderTestDataFactory.GetInlineData), MemberType = typeof(JsonSerializerBuilderTestDataFactory))]
    public void TestMethod(string[] values, int countProperties)
    {
        // Arrange
        var propertiesToIgnore = new List<string>(values);
        var dateFormatString = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK";
        var maxDepth = 10;
        var nullValueHandling = NullValueHandling.Ignore;
        var jsonSerializer = JsonSerializerBuilder.Build(propertiesToIgnore, dateFormatString, maxDepth, nullValueHandling);
        var entity = new TestEntity
        {
            Property1 = "Test",
            Property2 = new List<string> { "Value1", "Value2" },
            Property3 = new List<int> { 1, 2, 3 }
        };

        // Act
        var dynamoEntity = JObject.FromObject(entity, jsonSerializer);

        // Assert
        Assert.NotNull(dynamoEntity);
        Assert.Equal(countProperties, dynamoEntity.Count);
    }

    public class TestEntity
    {
        public string Property1 { get; set; }
        public List<string> Property2 { get; set; }
        public List<int> Property3 { get; set; }

        public TestEntity()
        {
            Property1 = string.Empty;
            Property2 = new List<string>();
            Property3 = new List<int>();
        }
    }
}
