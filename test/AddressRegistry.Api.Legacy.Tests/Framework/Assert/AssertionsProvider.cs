namespace AddressRegistry.Api.Legacy.Tests.Framework.Assert
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AddressMatch;
    using AddressRegistry.Api.Legacy.AddressMatch;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Legacy.AddressMatch.Responses;
    using Legacy.AddressMatch.V1.Matching;

    internal static class AssertionsProvider
    {
        public static SanitizationTaskAssertions<HouseNumberWithSubaddress> Should(this Task<List<HouseNumberWithSubaddress>> task)
            => new SanitizationTaskAssertions<HouseNumberWithSubaddress>(task);

        public static HuisnummerWithSubadresTaskAssertions Should(this Task<HouseNumberWithSubaddress> task)
            => new HuisnummerWithSubadresTaskAssertions(task);

        public static SanitizationTaskAssertions<AdresListFilterStub> Should(this Task<List<AdresListFilterStub>> task)
            => new SanitizationTaskAssertions<AdresListFilterStub>(task);

        public static AdresListFiltersTaskAssertions Should(this Task<AdresListFilterStub> task)
            => new AdresListFiltersTaskAssertions(task);

        public static AdresMatchCollectieAssertions Should(this AddressMatchCollection subject)
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
