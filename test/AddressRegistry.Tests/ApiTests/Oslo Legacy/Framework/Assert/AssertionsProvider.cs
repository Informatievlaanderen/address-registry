namespace AddressRegistry.Tests.ApiTests.Oslo_Legacy.Framework.Assert
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AddressMatch;
    using AddressRegistry.Api.Oslo.AddressMatch;
    using AddressRegistry.Api.Oslo.AddressMatch.Responses;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;

    internal static class AssertionsProvider
    {
        public static SanitizationTaskAssertions<HouseNumberWithSubaddress> Should(this Task<List<HouseNumberWithSubaddress>> task)
            => new SanitizationTaskAssertions<HouseNumberWithSubaddress>(task);

        public static HuisnummerWithSubadresTaskAssertions Should(this Task<HouseNumberWithSubaddress> task)
            => new HuisnummerWithSubadresTaskAssertions(task);

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
