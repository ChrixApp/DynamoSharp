namespace DynamoSharp.Tests.DynamoDb.QueryBuilder.PartiQL.Models;

public class PartiQLAddress
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }

    public PartiQLAddress(string street, string city, string state, string zipCode)
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