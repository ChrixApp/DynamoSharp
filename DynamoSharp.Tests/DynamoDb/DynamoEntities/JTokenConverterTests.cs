using DynamoSharp.DynamoDb.DynamoEntities;
using Newtonsoft.Json.Linq;

namespace DynamoSharp.Tests.DynamoDb.DynamoEntities;

public class JTokenConverterTests
{
    [Fact]
    public void ConvertToString_ReturnsEmpty_WhenTokenIsNull()
    {
        var result = JTokenConverter.ConvertToString(null);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ConvertToString_DateFormatsCorrectly()
    {
        var date = DateTime.SpecifyKind(new DateTime(2020, 1, 2, 3, 4, 5, 123).AddTicks(45), DateTimeKind.Utc);
        var token = new JValue(date);
        var expected = date.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK");
        var result = JTokenConverter.ConvertToString(token);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ConvertToString_Array_JoinsElementsWithHash()
    {
        var token = new JArray("a", "b", "c");
        var result = JTokenConverter.ConvertToString(token);
        Assert.Equal("a#b#c", result);
    }

    [Fact]
    public void ConvertToString_Default_ReturnsToStringForOtherTypes()
    {
        var token = new JValue("hello");
        var result = JTokenConverter.ConvertToString(token);
        Assert.Equal("hello", result);
    }
}
