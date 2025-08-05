using OneToManyWithCustomPrimaryKey.Exceptions;

namespace OneToManyWithCustomPrimaryKey.Models;

public class Order
{
    public Guid Id { get; private set; }
    public Guid BuyerId { get; private set; }
    public Address? Address { get; private set; }
    public Status Status { get; private set; }
    public DateTime Date { get; private set; }
    private readonly List<Item> _items = new List<Item>();

    public IReadOnlyCollection<Item> Items => _items;

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
            throw new ProductNotFoundException(productId);
        }

        _items.Remove(existingOrderForProduct);
    }

    public class Builder
    {
        private readonly Order _order;

        public Builder()
        {
            _order = new Order();
        }

        public Builder WithId(Guid id)
        {
            _order.Id = id;
            return this;
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

        public Builder WithStatus(Status status)
        {
            _order.Status = status;
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
            if (_order.Id == Guid.Empty)
                throw new ArgumentException("Id cannot be empty");

            if (_order.BuyerId == Guid.Empty)
                throw new ArgumentException("BuyerId cannot be empty");

            if (_order.Address == null)
                throw new ArgumentException("Address cannot be null");

            if (_order.Date == DateTime.MinValue)
                throw new ArgumentException("Date cannot be empty");
        }
    }
}
