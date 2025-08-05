namespace OptimisticLockingUpdate.Models;

public class Item
{
    public Guid Id { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Units { get; private set; }

    public void AddUnits(int units)
    {
        Units += units;
    }

    public void ChangePrice(decimal newPrice)
    {
        UnitPrice = newPrice;
    }

    public class Builder
    {
        private readonly Item _orderItem;

        public Builder()
        {
            _orderItem = new Item();
        }

        public Builder WithId(Guid id)
        {
            _orderItem.Id = id;
            return this;
        }

        public Builder WithProductName(string productName)
        {
            _orderItem.ProductName = productName;
            return this;
        }

        public Builder WithUnitPrice(decimal unitPrice)
        {
            _orderItem.UnitPrice = unitPrice;
            return this;
        }

        public Builder WithUnits(int units)
        {
            _orderItem.Units = units;
            return this;
        }

        public Item Build()
        {
            Validate();
            return _orderItem;
        }

        private void Validate()
        {
            if (_orderItem.Id == Guid.Empty)
                throw new ArgumentException("ProductId cannot be empty");

            if (string.IsNullOrWhiteSpace(_orderItem.ProductName))
                throw new ArgumentException("ProductName cannot be empty");

            if (_orderItem.UnitPrice <= 0)
                throw new ArgumentException("UnitPrice must be greater than 0");

            if (_orderItem.Units <= 0)
                throw new ArgumentException("Units must be greater than 0");
        }
    }
}
