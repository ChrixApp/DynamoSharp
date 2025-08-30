using DynamoSharp.Converters.Objects;
using DynamoSharp.Tests.Contexts.Models;
using System.Runtime.CompilerServices;

namespace DynamoSharp.Tests.Converters.Objects;

public class ReflectionUtilsTests
{
    [Fact]
    public void SetValue_SetsPublicProperty()
    {
        // Arrange
        var order = new Order.Builder()
            .WithAddress("test", "test", "test", "12345")
            .WithBuyerId(Guid.NewGuid())
            .WithDate(DateTime.Now)
            .Build();

        var newBuyerId = Guid.NewGuid();

        // Act
        ReflectionUtils.SetValue(order, typeof(Order), nameof(Order.BuyerId), newBuyerId);

        // Assert
        Assert.Equal(newBuyerId, order.BuyerId);
    }

    [Fact]
    public void SetValue_SetsPrivateFieldFromReadOnlyProperty()
    {
        // Arrange
        var order = new Order.Builder()
            .WithAddress("test", "test", "test", "12345")
            .WithBuyerId(Guid.NewGuid())
            .WithDate(DateTime.Now)
            .Build();

        var newItems = new List<Item>();
        newItems.Add(new Item.Builder()
            .WithId(Guid.NewGuid())
            .WithProductName("Test Product")
            .WithUnitPrice(10)
            .WithUnits(1)
            .Build());
        newItems.Add(new Item.Builder()
            .WithId(Guid.NewGuid())
            .WithProductName("Test Product 2")
            .WithUnitPrice(20)
            .WithUnits(2)
            .Build());

        // Act
        ReflectionUtils.SetValue(order, typeof(Order), nameof(Order.Items), newItems);

        // Assert
        Assert.Equal(newItems.Count, order.Items.Count);
        Assert.Equal(newItems[0].Id, order.Items[0].Id);
        Assert.Equal(newItems[0].ProductName, order.Items[0].ProductName);
        Assert.Equal(newItems[0].UnitPrice, order.Items[0].UnitPrice);
        Assert.Equal(newItems[0].Units, order.Items[0].Units);
        Assert.Equal(newItems[1].Id, order.Items[1].Id);
        Assert.Equal(newItems[1].ProductName, order.Items[1].ProductName);
        Assert.Equal(newItems[1].UnitPrice, order.Items[1].UnitPrice);
        Assert.Equal(newItems[1].Units, order.Items[1].Units);
    }

    [Fact]
    public void SetValue_IgnoresNonExistentProperty()
    {
        // Arrange
        var order = new Order.Builder()
            .WithAddress("test", "test", "test", "12345")
            .WithBuyerId(Guid.NewGuid())
            .WithDate(DateTime.Now)
            .Build();

        var originalBuyerId = order.BuyerId;
        var bogusPropertyValue = Guid.NewGuid();

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => ReflectionUtils.SetValue(order, typeof(Order), "NonExistentProp", bogusPropertyValue));
        Assert.Equal(originalBuyerId, order.BuyerId);
    }

    [Fact]
    public void SetValue_WhenPropertyIsInherited_ShouldSetValueToObject()
    {
        // Arrange
        var childType = typeof(Child);
        var child = (Child)RuntimeHelpers.GetUninitializedObject(childType);
        var newId = Guid.NewGuid();

        // Act
        ReflectionUtils.SetValue(child, childType, nameof(Child.Id), newId);

        // Assert
        Assert.Equal(newId, child.Id);
    }

    public class GrandParent
    {
        public Guid Id { get; private set; }

        public GrandParent(Guid id)
        {
            Id = id;
        }
    }

    public class Parent : GrandParent
    {
        public string FirstName { get; private set; }

        public Parent(Guid id, string firstName) : base(id)
        {
            FirstName = firstName;
        }
    }

    public class Child : Parent
    {
        public string LastName { get; private set; }

        public Child(Guid id, string firstName, string lastName) : base(id, firstName)
        {
            LastName = lastName;
        }
    }
}
