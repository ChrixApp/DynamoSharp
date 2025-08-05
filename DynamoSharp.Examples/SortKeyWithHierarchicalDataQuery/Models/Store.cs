namespace SortKeyWithHierarchicalDataQuery.Models;

public class Store
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public Address? Address { get; private set; }

    public class Builder
    {
        private readonly Store _store;

        public Builder()
        {
            _store = new Store();
        }

        public Builder WithId(Guid id)
        {
            _store.Id = id;
            return this;
        }

        public Builder WithName(string name)
        {
            _store.Name = name;
            return this;
        }

        public Builder WithPhone(string phone)
        {
            _store.Phone = phone;
            return this;
        }

        public Builder WithEmail(string email)
        {
            _store.Email = email;
            return this;
        }

        public Builder WithAddress(string street, string city, string state, string zipCode, string country)
        {
            _store.Address = new Address(street, city, state, zipCode, country);
            return this;
        }

        public Store Build()
        {
            Validate();
            return _store;
        }

        public void Validate()
        {
            if (_store.Id == Guid.Empty)
            {
                throw new InvalidOperationException("Id is required");
            }

            if (string.IsNullOrWhiteSpace(_store.Name))
            {
                throw new InvalidOperationException("Name is required");
            }

            if (string.IsNullOrWhiteSpace(_store.Phone))
            {
                throw new InvalidOperationException("Phone is required");
            }

            if (string.IsNullOrWhiteSpace(_store.Email))
            {
                throw new InvalidOperationException("Email is required");
            }

            if (_store.Address == null)
            {
                throw new InvalidOperationException("Address is required");
            }
        }
    }
}
