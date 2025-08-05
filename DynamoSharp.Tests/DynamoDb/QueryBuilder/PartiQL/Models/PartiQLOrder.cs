namespace DynamoSharp.Tests.DynamoDb.QueryBuilder.PartiQL.Models;

public class PartiQLOrder
{
    public string PartitionKey { get; private set; } = string.Empty;
    public string SortKey { get; private set; } = string.Empty;
    public Guid Id { get; private set; }
    public Guid BuyerId { get; private set; }
    public PartiQLAddress? Address { get; private set; }
    public PartiQLStatus Status { get; private set; }
    public DateTime Date { get; private set; }
    private readonly List<PartiQLItem> _items = new List<PartiQLItem>();

    public IReadOnlyCollection<PartiQLItem> Items => _items;
    public decimal Total => _items.Sum(i => i.UnitPrice * i.Units);

    public void UpdateAddress(string street, string city, string state, string zipCode)
    {
        Address = new PartiQLAddress(street, city, state, zipCode);
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

        var orderItem = new PartiQLItem.Builder()
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
        private readonly PartiQLOrder _order;

        public Builder()
        {
            _order = new PartiQLOrder();
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
            _order.Address = new PartiQLAddress(street, city, state, zipCode);
            return this;
        }

        public Builder WithStatus(PartiQLStatus status)
        {
            _order.Status = status;
            return this;
        }

        public Builder WithDate(DateTime date)
        {
            _order.Date = date;
            return this;
        }

        public PartiQLOrder Build()
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
