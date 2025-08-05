namespace SortKeyWithHierarchicalDataQuery.Models;

public class Address
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }
    public string Country { get; private set; }

    public Address(string street, string city, string state, string zipCode, string country)
    {
        ArgumentNullException.ThrowIfNull(street);
        ArgumentNullException.ThrowIfNull(city);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(zipCode);
        ArgumentNullException.ThrowIfNull(country);

        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
    }
}
