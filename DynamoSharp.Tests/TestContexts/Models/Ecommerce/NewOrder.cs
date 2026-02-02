namespace DynamoSharp.Tests.TestContexts.Models.Ecommerce;

public class NewOrder
{
    public Guid Id { get; private set; }
    public Guid BuyerId { get; private set; }
    public Address? Address { get; private set; }
    public DateTime Date { get; private set; }
    private List<Item> _items = new List<Item>();
    public IReadOnlyList<Item> Items
    {
        get => _items.AsReadOnly();
        private set => _items = value.ToList();
    }
    public decimal Total => _items.Sum(i => i.Total);

    public NewOrder(Guid id, Guid buyerId, Address address, DateTime date)
    {
        Id = id;
        BuyerId = buyerId;
        Address = address;
        Date = date;
        Items = new List<Item>();
    }
}
