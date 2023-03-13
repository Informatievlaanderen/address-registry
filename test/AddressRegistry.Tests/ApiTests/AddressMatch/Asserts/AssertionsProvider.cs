namespace AddressRegistry.Tests.ApiTests.AddressMatch.Asserts
{
    using AddressRegistry.Api.Oslo.AddressMatch.Responses;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;

    internal static class AssertionsProvider
    {
        public static AdresMatchCollectieAssertions Should(this AddressMatchOsloCollection subject)
            => new AdresMatchCollectieAssertions(subject);

        public static AdresMatchItemAssertions Should(this AdresMatchItem subject)
            => new AdresMatchItemAssertions(subject);

        public static AdresMatchItemGemeenteAssertions Should(this AdresMatchItemGemeente subject)
            => new AdresMatchItemGemeenteAssertions(subject);

        public static AdresMatchItemStraatnaamAssertions Should(this AdresMatchItemStraatnaam subject)
            => new AdresMatchItemStraatnaamAssertions(subject);

        public static VolledigAdresAssertions Should(this VolledigAdres subject)
            => new VolledigAdresAssertions(subject);
    }
}
