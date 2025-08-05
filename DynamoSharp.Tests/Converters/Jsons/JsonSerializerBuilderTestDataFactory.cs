namespace DynamoSharp.Tests.Converters.Jsons;

public static class JsonSerializerBuilderTestDataFactory
{
    public static IEnumerable<object[]> GetInlineData()
    {
        yield return new object[]
        {
        new string[] { "Property2", "Property3" },
        1
        };
        yield return new object[]
        {
        new string[] { "Property2" },
        2
        };
        yield return new object[]
        {
        new string[] { },
        3
        };
    }
}
