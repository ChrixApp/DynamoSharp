using DynamoSharp.Converters.Jsons;
using EfficientDynamoDb.DocumentModel;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DynamoSharp.Tests.Converters.Jsons;

public class JObjectToDynamoDbConverterTests
{
    private readonly JObjectToDynamoDbConverter _converter;

    public JObjectToDynamoDbConverterTests()
    {
        _converter = JObjectToDynamoDbConverter.Instance;
    }

    [Fact]
    public void Instance_ShouldReturnSingletonInstance()
    {
        // Act
        var instance1 = JObjectToDynamoDbConverter.Instance;
        var instance2 = JObjectToDynamoDbConverter.Instance;

        // Assert
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void ConvertJTokenToAttributeValue_ShouldConvertTokenUsingStrategy()
    {
        // Arrange
        var token = new JValue(123);
        var mockConverter = new Mock<ITokenConverter>();
        mockConverter.Setup(c => c.Convert(token)).Returns(new NumberAttributeValue("123"));

        var strategies = new Dictionary<JTokenType, ITokenConverter>
        {
            { JTokenType.Integer, mockConverter.Object }
        };

        var converter = JObjectToDynamoDbConverter.Instance;

        // Act
        var attributeValue = converter.ConvertJTokenToAttributeValue(token);

        // Assert
        Assert.Equal("123", attributeValue.AsNumberAttribute().Value);
    }

    [Fact]
    public void ConvertJTokenToAttributeValue_ShouldThrowExceptionForUnsupportedTokenType()
    {
        // Arrange
        var token = new JValue(new Uri("http://example.com"));

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _converter.ConvertJTokenToAttributeValue(token));
        Assert.Equal($"Unsupported JTokenType: {token.Type}", exception.Message);
    }


    [Fact]
    public void ConvertJObjectToDocument_ShouldConvertJObjectToDocument()
    {
        // Arrange
        var jObject = new JObject
        {
            { "Name", "John Doe" },
            { "Age", 30 }
        };

        var mockStringConverter = new Mock<ITokenConverter>();
        mockStringConverter.Setup(c => c.Convert(It.IsAny<JToken>())).Returns(new StringAttributeValue("John Doe"));

        var mockIntegerConverter = new Mock<ITokenConverter>();
        mockIntegerConverter.Setup(c => c.Convert(It.IsAny<JToken>())).Returns(new NumberAttributeValue("30"));

        var converter = JObjectToDynamoDbConverter.Instance;

        // Act
        var document = converter.ConvertJObjectToDocument(jObject);

        // Assert
        Assert.NotNull(document);
        Assert.Equal("John Doe", document["Name"]);
        Assert.Equal("30", document["Age"].AsNumberAttribute().Value);
    }
}
