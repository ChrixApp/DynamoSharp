namespace OneToManyWithCustomPrimaryKeyQueryPk.Models;

public class Address
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }

    public Address(string street, string city, string state, string zipCode)
    {
        ArgumentNullException.ThrowIfNull(street);
        ArgumentNullException.ThrowIfNull(city);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(zipCode);

        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
    }
}