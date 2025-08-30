using DynamoSharp.DynamoDb.ModelsBuilder;
using System.Linq.Expressions;

namespace DynamoSharp.Tests.DynamoDb.ModelsBuilder;

public class ExpressionHandlerTests
{
    [Fact]
    public void FindPath_WithMemberAccess_ShouldReturnPath()
    {
        // Arrange
        Expression<Func<MyClass, object>> memberLamda = x => x.MyObject!;

        // Act
        var result = ExpressionHandler.FindPath(memberLamda);

        // Assert
        Assert.Equal("MyObject", result);
    }

    [Fact]
    public void FindPath_WitTypeAs_ShouldReturnPath()
    {
        // Arrange
        Expression<Func<MyClass, object>> typeAsLamda = x => x.MyObject! as object;

        // Act
        var result = ExpressionHandler.FindPath(typeAsLamda);

        // Assert
        Assert.Equal("MyObject", result);
    }

    [Fact]
    public void FindPath_WithConvert_ShouldReturnPath()
    {
        // Arrange
        Expression<Func<MyClass, object>> convertLamda = x => (object?)x.MyObject!;

        // Act
        var result = ExpressionHandler.FindPath(convertLamda);

        // Assert
        Assert.Equal("MyObject", result);
    }

    public class MyClass
    {
        public object? MyObject { get; set; }
    }
}

