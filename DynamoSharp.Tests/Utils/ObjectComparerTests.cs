using DynamoSharp.Utils;
using FluentAssertions;
using Xunit;

namespace DynamoSharp.Tests.Utils;

public class ObjectComparerTests
{
    private class SimpleClass
    {
        public int Number { get; set; }
        public string? Text { get; set; }
    }

    private class ComplexClass
    {
        public SimpleClass? InnerObject { get; set; }
    }

    [Fact]
    public void HaveSameNestedReferences_WithSameReferenceForSimpleProperties_ShouldReturnTrue()
    {
        var obj1 = new SimpleClass { Number = 1, Text = "Hello" };
        var obj2 = obj1;

        var result = ObjectComparer.HaveSameNestedReferences(obj1, obj2);

        result.Should().BeTrue();
    }

    [Fact]
    public void HaveSameNestedReferences_WithDifferentReferencesForSimpleProperties_ShouldReturnFalse()
    {
        var obj1 = new SimpleClass { Number = 1, Text = "Hello" };
        var obj2 = new SimpleClass { Number = 1, Text = "World" };

        var result = ObjectComparer.HaveSameNestedReferences(obj1, obj2);

        result.Should().BeFalse();
    }

    [Fact]
    public void HaveSameNestedReferences_WithSameReferenceForNestedProperties_ShouldReturnTrue()
    {
        var innerObject = new SimpleClass { Number = 42, Text = "Inner" };
        var obj1 = new ComplexClass { InnerObject = innerObject };
        var obj2 = new ComplexClass { InnerObject = innerObject };

        var result = ObjectComparer.HaveSameNestedReferences(obj1, obj2);

        result.Should().BeTrue();
    }

    [Fact]
    public void HaveSameNestedReferences_WithDifferentReferencesForNestedProperties_ShouldReturnFalse()
    {
        var obj1 = new ComplexClass { InnerObject = new SimpleClass { Number = 42, Text = "Inner" } };
        var obj2 = new ComplexClass { InnerObject = new SimpleClass { Number = 42, Text = "Inner" } };

        var result = ObjectComparer.HaveSameNestedReferences(obj1, obj2);

        result.Should().BeFalse();
    }

    [Fact]
    public void HaveSameNestedReferences_WithCircularReferences_ShouldHandleCorrectly()
    {
        var obj1 = new ComplexClass();
        var obj2 = new ComplexClass();
        obj1.InnerObject = new SimpleClass { Number = 1, Text = "Hello" };
        obj2.InnerObject = obj1.InnerObject;

        var result = ObjectComparer.HaveSameNestedReferences(obj1, obj2);

        result.Should().BeTrue();
    }

    [Fact]
    public void HaveSameNestedReferences_WithBothObjectsNull_ShouldReturnTrue()
    {
        SimpleClass? obj1 = null;
        SimpleClass? obj2 = null;

        var result = ObjectComparer.HaveSameNestedReferences(obj1, obj2);

        result.Should().BeTrue();
    }

    [Fact]
    public void HaveSameNestedReferences_WithOneObjectNull_ShouldReturnFalse()
    {
        SimpleClass obj1 = new SimpleClass { Number = 1, Text = "Hello" };
        SimpleClass? obj2 = null;

        var result = ObjectComparer.HaveSameNestedReferences(obj1, obj2);

        result.Should().BeFalse();
    }

    [Fact]
    public void HaveSameNestedReferences_WithSelfReferencingObject_ShouldReturnTrue()
    {
        var obj1 = new ComplexClass();
        obj1.InnerObject = new SimpleClass { Number = 1, Text = "Hello" };

        var result = ObjectComparer.HaveSameNestedReferences(obj1, obj1);

        result.Should().BeTrue();
    }
}

