using DynamoSharp.DynamoDb.DynamoEntities;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DynamoSharp.Tests.DynamoDb.DynamoEntities;

public class KeyBuilderTests
{
    [Fact]
    public void BuildKey_GivenEmptyDictionary_ReturnsEmptyString()
    {
        // Arrange
        var keyPaths = new Dictionary<string, string>();
        var jObject = JObject.Parse("{}");

        // Act
        var result = KeyBuilder.BuildKey(keyPaths, jObject);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void BuildKey_GivenMissingToken_NoPartsAdded()
    {
        // Arrange
        var keyPaths = new Dictionary<string, string> { { "missing.path", "prefix" } };
        var jObject = JObject.Parse("{ \"different\": \"value\" }");

        // Act
        var result = KeyBuilder.BuildKey(keyPaths, jObject);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void BuildKey_GivenTokenWithNoPrefix_UsesTokenValueOnly()
    {
        // Arrange
        var keyPaths = new Dictionary<string, string> { { "name", "" } };
        var jObject = JObject.Parse("{ \"name\": \"Alice\" }");

        // Act
        var result = KeyBuilder.BuildKey(keyPaths, jObject);

        // Assert
        Assert.Equal("Alice", result);
    }

    [Fact]
    public void BuildKey_GivenTokenAndPrefix_IncludesPrefixAndTokenValue()
    {
        // Arrange
        var keyPaths = new Dictionary<string, string> { { "name", "person" } };
        var jObject = JObject.Parse("{ \"name\": \"Alice\" }");

        // Act
        var result = KeyBuilder.BuildKey(keyPaths, jObject);

        // Assert
        Assert.Equal("person#Alice", result);
    }

    [Fact]
    public void BuildKey_GivenDateToken_FormatsDateCorrectly()
    {
        // Arrange
        var dateString = "2023-05-01T12:34:56.789Z";
        var keyPaths = new Dictionary<string, string> { { "created", "" } };
        var jObject = JObject.Parse($"{{ \"created\": \"{dateString}\" }}");

        // Act
        var result = KeyBuilder.BuildKey(keyPaths, jObject);

        // Assert
        // The final part includes the UTC offset, so just check substring.
        Assert.Contains("2023-05-01T12:34:56.789", result);
    }

    [Fact]
    public void BuildKey_GivenMultipleKeyPaths_BuildsConcatenatedResult()
    {
        // Arrange
        var keyPaths = new Dictionary<string, string>
            {
                {"firstName", "first"},
                {"lastName", "last"}
            };
        var jObject = JObject.Parse("{ \"firstName\": \"Alice\", \"lastName\": \"Wonderland\" }");

        // Act
        var result = KeyBuilder.BuildKey(keyPaths, jObject);

        // Assert
        Assert.Equal("first#Alice#last#Wonderland", result);
    }
}
