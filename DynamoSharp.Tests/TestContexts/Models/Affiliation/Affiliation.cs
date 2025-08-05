namespace DynamoSharp.Tests.Contexts.Models.Affiliation;

public class Affiliation
{
    public Guid Id { get; private set; }
    public Guid MerchantId { get; private set; }
    public Guid TerminalId { get; private set; }
    public Section Section { get; private set; }
    public CardBrand CardBrand { get; private set; }
    public CountryOrRigion CountryOrRigion { get; private set; }
    public Bank Bank { get; private set; }
    public AffiliationType Type { get; private set; }
    public float Percentage { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Affiliation(
        Guid merchantId,
        Guid terminalId,
        Section section,
        CardBrand cardBrand,
        CountryOrRigion countryOrRigion,
        Bank bank,
        AffiliationType type,
        float percentage = 0)
    {
        Id = Guid.NewGuid();
        MerchantId = merchantId;
        TerminalId = terminalId;
        Section = section;
        CardBrand = cardBrand;
        CountryOrRigion = countryOrRigion;
        Bank = bank;
        Type = type;
        Percentage = percentage;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateCountryOrRigion(CountryOrRigion countryOrRigion)
    {
        CountryOrRigion = countryOrRigion;
    }
}