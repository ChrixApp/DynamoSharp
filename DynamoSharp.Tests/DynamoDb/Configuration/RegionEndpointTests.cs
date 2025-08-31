using DynamoSharp.DynamoDb.Configs;
using System.Reflection;

namespace DynamoSharp.Tests.DynamoDb.Configuration;

public class RegionEndpointTests
{
    [Fact]
    public void USEast1_ShouldHaveExpectedRegionAndRequestUri()
    {
        var endpoint = RegionEndpoint.USEast1;
        Assert.Equal("us-east-1", endpoint.Region);

        var requestUri = (string)typeof(RegionEndpoint)
            .GetProperty("RequestUri", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!
            .GetValue(endpoint)!;

        Assert.Equal("https://dynamodb.us-east-1.amazonaws.com", requestUri);
    }

    [Fact]
    public void CNNorth1_ShouldUseChinaEndpointFormat()
    {
        var endpoint = RegionEndpoint.CNNorth1;
        Assert.Equal("cn-north-1", endpoint.Region);

        var requestUri = (string)typeof(RegionEndpoint)
            .GetProperty("RequestUri", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!
            .GetValue(endpoint)!;

        Assert.Equal("https://dynamodb.cn-north-1.amazonaws.com.cn", requestUri);
    }

    [Fact]
    public void ServiceName_Constant_IsDynamodb()
    {
        var field = typeof(RegionEndpoint).GetField("ServiceName", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.NotNull(field);

        var value = (string)field!.GetValue(null)!;
        Assert.Equal("dynamodb", value);
    }

    [Fact]
    public void AllPublicStaticRegionProperties_ShouldHaveValidRequestUri()
    {
        var regionProps = typeof(RegionEndpoint)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.PropertyType == typeof(RegionEndpoint))
            .ToArray();

        Assert.NotEmpty(regionProps);

        var requestUriProp = typeof(RegionEndpoint).GetProperty("RequestUri", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.NotNull(requestUriProp);

        foreach (var prop in regionProps)
        {
            var instance = (RegionEndpoint)prop.GetValue(null)!;
            Assert.False(string.IsNullOrWhiteSpace(instance.Region));

            var requestUri = (string)requestUriProp!.GetValue(instance)!;
            var expected = instance.Region.StartsWith("cn-")
                ? $"https://dynamodb.{instance.Region}.amazonaws.com.cn"
                : $"https://dynamodb.{instance.Region}.amazonaws.com";

            Assert.Equal(expected, requestUri);
        }
    }

    [Fact]
    public void StaticProperties_ReturnDistinctInstances()
    {
        var a = RegionEndpoint.USEast1;
        var b = RegionEndpoint.USEast1;
        Assert.NotSame(a, b); // verifies the properties create new instances (not singletons)
    }
}
