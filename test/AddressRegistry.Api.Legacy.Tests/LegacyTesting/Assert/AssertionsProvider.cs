namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting.Assert
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AddressMatch.Matching;
    using AddressMatch.Responses;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;

    public static class AssertionsProvider
    {
        public static SanitizationTaskAssertions<HouseNumberWithSubaddress> Should(this Task<List<HouseNumberWithSubaddress>> task)
        {
            return new SanitizationTaskAssertions<HouseNumberWithSubaddress>(task);
        }

        public static HuisnummerWithSubadresTaskAssertions Should(this Task<HouseNumberWithSubaddress> task)
        {
            return new HuisnummerWithSubadresTaskAssertions(task);
        }

        public static SanitizationTaskAssertions<AdresListFilterStub> Should(this Task<List<AdresListFilterStub>> task)
        {
            return new SanitizationTaskAssertions<AdresListFilterStub>(task);
        }

        public static AdresListFiltersTaskAssertions Should(this Task<AdresListFilterStub> task)
        {
            return new AdresListFiltersTaskAssertions(task);
        }

        public static AdresMatchCollectieAssertions Should(this AdresMatchCollectie subject)
        {
            return new AdresMatchCollectieAssertions(subject);
        }

        public static AdresMatchItemAssertions Should(this AdresMatchItem subject)
        {
            return new AdresMatchItemAssertions(subject);
        }

        public static AdresMatchItemGemeenteAssertions Should(this AdresMatchItemGemeente subject)
        {
            return new AdresMatchItemGemeenteAssertions(subject);
        }

        public static AdresMatchItemStraatnaamAssertions Should(this AdresMatchItemStraatnaam subject)
        {
            return new AdresMatchItemStraatnaamAssertions(subject);
        }

        public static VolledigAdresAssertions Should(this VolledigAdres subject)
        {
            return new VolledigAdresAssertions(subject);
        }
    }
}
