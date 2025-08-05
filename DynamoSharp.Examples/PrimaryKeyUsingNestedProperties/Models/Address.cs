namespace PrimaryKeyUsingNestedProperties.Models;

public class Address
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }
    public string Country { get; private set; }

    public Address(string street, string city, string state, string zipCode, string country)
    {
        ArgumentNullException.ThrowIfNull(street, nameof(street));
        ArgumentNullException.ThrowIfNull(city, nameof(city));
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(zipCode, nameof(zipCode));
        ArgumentNullException.ThrowIfNull(country, nameof(country));

        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
    }
}
