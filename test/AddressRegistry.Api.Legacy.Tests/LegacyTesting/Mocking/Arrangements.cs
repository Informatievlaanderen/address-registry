namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting.Mocking
{
    using System;
    using AddressMatch.Matching;
    using Generate;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.PostalInfo;
    using Projections.Syndication.StreetName;
    using System.Collections.Generic;
    using Projections.Legacy.AddressDetail;

    internal static class Arrangements
    {
        public static IEnumerable<StreetNameLatestItem> ArrangeKadStraatnamen(this Mocking<IKadRrService, KadRrServiceSetup, KadRrServiceVerification> mock,
            string nisCode, string gemeentenaam, int straatnaamId, string straatnaam, string kadStraatcode)
        {
            return mock.Arrange(
            Produce.One(
                Generate.tblStraatNaam
                .Select(s => s.WithOsloId(straatnaamId.ToString())
                .WithStraatNaam(straatnaam)
                .WithGemeenteId(nisCode)
                )),
                (when, x) => when.StraatnamenExistForKadStraatcodeAndNISCode(x, kadStraatcode, nisCode));
        }

        public static StreetNameLatestItem ArrangeRrStraatnaam(this Mocking<IKadRrService, KadRrServiceSetup, KadRrServiceVerification> mock,
            string postcode, string nisCode, string gemeentenaam, int straatnaamId, string straatnaam, string rrStraatcode)
        {
            return mock.Arrange(
                Generate.tblStraatNaam
                .Select(s => s.WithOsloId(straatnaamId.ToString())
                .WithStraatNaam(straatnaam)
                .WithGemeenteId(nisCode)),
                (when, x) => when.StraatnaamExistsForRrStraatcodeAndPostcode(x, rrStraatcode, postcode));
        }

        public static IEnumerable<MunicipalityLatestItem> ArrangeLatestGemeente(this Mocking<ILatestQueries, LatestQueriesSetup, LatestQueriesVerification> mock, string nisCode, string gemeentenaam)
        {
            return mock.Arrange(
                Produce.One(Generate.tblGemeente.Select(
                    g => g.WithNisGemeenteCode(nisCode)
                    .WithGemeenteNaam(gemeentenaam))),
                (when, x) => when.LatestGemeentesExist(x));
        }

        public static IEnumerable<PostalInfoLatestItem> ArrangeLatestPostInfo(this Mocking<ILatestQueries, LatestQueriesSetup, LatestQueriesVerification> mock, string nisCode, string postcode)
        {
            return mock.Arrange(
                Produce.One(Generate.tblPostInfo.Select(
                    g => g.WithNISCode(nisCode)
                        .WithPostcode(postcode))),
                (when, x) => when.LatestPostInfo(x));
        }

        public static IEnumerable<PostalInfoLatestItem> ArrangeLatestPostInfoForPartOfMunicipality(this Mocking<ILatestQueries, LatestQueriesSetup, LatestQueriesVerification> mock, string nisCode, string partName)
        {
            return mock.Arrange(
                Produce.One(Generate.tblPostInfo.Select(
                    g => g.WithNISCode(nisCode)
                        .WithPostnaam(partName))),
                (when, x) => when.LatestPostInfo(x));
        }

        public static IEnumerable<StreetNameLatestItem> ArrangeLatestStraatnaam(this Mocking<ILatestQueries, LatestQueriesSetup, LatestQueriesVerification> mock, string nisCode, string gemeentenaam, string straatnaamId, string straatNaam, Guid streetNameId)
        {
            return mock.Arrange(Produce.One(Generate.tblStraatNaam
                .Select(sn => sn.WithStraatNaam(straatNaam)
                .WithOsloId(straatnaamId)
                .WithStraatNaamId(streetNameId)
                .WithGemeenteId(nisCode))),
                (when, x) => when.LatestStraatnamenExistForGemeentenaam(x, gemeentenaam));
        }

        public static IEnumerable<AddressDetailItem> ArrangeLatestAdres(this Mocking<ILatestQueries, LatestQueriesSetup, LatestQueriesVerification> mock, MunicipalityLatestItem gemeente, StreetNameLatestItem straatNaam,
            string postcode, string huisnummer, string busnummer)
        {
            return mock.Arrange(Produce.One(Generate.tblHuisNummer
                .Select(a => a
                            .WithStraatNaamId(straatNaam.StreetNameId)
                            .WithTblPostKanton(postcode)
                            .WithHuisNummer(huisnummer)
                            .WithBusnummer(busnummer))),
                (when, x) => when.LatestAdressenExist(x, straatNaam.OsloId, huisnummer, busnummer));
        }
    }
}
