using DynamoSharp.Tests.Contexts.Models;

namespace DynamoSharp.Tests.TestContexts.Models.Ecommerce;

public class Order : IAggregateRoot
{
    public Guid Id { get; private set; }
    public Guid BuyerId { get; private set; }
    public Address? Address { get; private set; }
    public DateTime Date { get; private set; }
    private readonly List<Item> _items = [];

    public IReadOnlyList<Item> Items => _items.AsReadOnly();
    public decimal Total => _items.Sum(i => i.Total);

    public void UpdateAddress(string street, string city, string state, string zipCode)
    {
        Address = new Address(street, city, state, zipCode);
    }

    public void AddProduct(Guid productId, string productName, decimal unitPrice, int units = 1)
    {
        var existingOrderForProduct = _items
            .SingleOrDefault(o => o.Id == productId);

        if (existingOrderForProduct != null)
        {
            existingOrderForProduct.ChangePrice(unitPrice);
            existingOrderForProduct.AddUnits(units);
            return;
        }

        var orderItem = new Item.Builder()
            .WithId(productId)
            .WithProductName(productName)
            .WithUnitPrice(unitPrice)
            .WithUnits(units)
            .Build();
        _items.Add(orderItem);
    }

    public void RemoveProduct(Guid productId)
    {
        var existingOrderForProduct = _items
            .SingleOrDefault(o => o.Id == productId);

        if (existingOrderForProduct == null)
        {
            throw new Exception($"Order does not contain an item with product id {productId}");
        }

        _items.Remove(existingOrderForProduct);
    }

    public class Builder
    {
        private readonly Order _order;

        public Builder()
        {
            _order = new Order();
            _order.Id = Guid.NewGuid();
        }

        public Builder WithBuyerId(Guid buyerId)
        {
            _order.BuyerId = buyerId;
            return this;
        }

        public Builder WithAddress(string street, string city, string state, string zipCode)
        {
            _order.Address = new Address(street, city, state, zipCode);
            return this;
        }

        public Builder WithDate(DateTime date)
        {
            _order.Date = date;
            return this;
        }

        public Order Build()
        {
            Validate();
            return _order;
        }

        public void Validate()
        {
            if (_order.BuyerId == Guid.Empty)
                throw new ArgumentException("BuyerId cannot be empty");

            if (_order.Address == null)
                    throw new ArgumentException("Address cannot be null");

            if (_order.Date == DateTime.MinValue)
                throw new ArgumentException("Date cannot be empty");
        }
    }
}
