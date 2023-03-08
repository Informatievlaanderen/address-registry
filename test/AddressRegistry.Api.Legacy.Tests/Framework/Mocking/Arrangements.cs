namespace AddressRegistry.Api.Legacy.Tests.Framework.Mocking
{
    using System;
    using System.Collections.Generic;
    using Generate;
    using Legacy.AddressMatch.V1.Matching;
    using Projections.Legacy.AddressDetail;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.PostalInfo;
    using Projections.Syndication.StreetName;

    internal static class Arrangements
    {
        public static IEnumerable<StreetNameLatestItem> ArrangeKadStraatnamen(
            this Mocking<IKadRrService, KadRrServiceSetup, KadRrServiceVerification> mock,
            string nisCode,
            string gemeentenaam,
            int straatnaamId,
            string straatnaam,
            string kadStraatcode)
        {
            return mock.Arrange(
                Produce.One(
                    Generate.TblStraatNaam
                        .Select(s => s
                            .WithPersistentLocalId(straatnaamId.ToString())
                            .WithStraatNaam(straatnaam)
                            .WithGemeenteId(nisCode))),
                (when, x) => when.StraatnamenExistForKadStraatcodeAndNisCode(x, kadStraatcode, nisCode));
        }

        public static StreetNameLatestItem ArrangeRrStraatnaam(
            this Mocking<IKadRrService, KadRrServiceSetup, KadRrServiceVerification> mock,
            string postcode,
            string nisCode,
            string gemeentenaam,
            int straatnaamId,
            string straatnaam,
            string rrStraatcode)
        {
            return mock.Arrange(
                Generate.TblStraatNaam
                    .Select(s => s
                        .WithPersistentLocalId(straatnaamId.ToString())
                        .WithStraatNaam(straatnaam)
                        .WithGemeenteId(nisCode)),
                (when, x) => when.StraatnaamExistsForRrStraatcodeAndPostcode(x, rrStraatcode, postcode));
        }

        public static IEnumerable<MunicipalityLatestItem> ArrangeLatestGemeente(
            this Mocking<ILatestQueries, LatestQueriesSetup, LatestQueriesVerification> mock,
            string nisCode,
            string gemeentenaam)
        {
            return mock.Arrange(
                Produce.One(Generate.TblGemeente
                    .Select(g => g
                        .WithNisGemeenteCode(nisCode)
                        .WithGemeenteNaam(gemeentenaam))),
                (when, x) => when.LatestGemeentesExist(x));
        }

        public static IEnumerable<MunicipalityLatestItem> ArrangeLatestGemeenteWithRandomNisCodes(
            this Mocking<ILatestQueries, LatestQueriesSetup, LatestQueriesVerification> mock,
            string gemeentenaam,
            int count)
        {
            return mock.Arrange(
                Produce.Exactly(count, Generate.TblGemeente
                    .Select(g => g.WithGemeenteNaam(gemeentenaam))),
                (when, x) => when.LatestGemeentes(x));
        }

        public static IEnumerable<PostalInfoLatestItem> ArrangeLatestPostInfo(
            this Mocking<ILatestQueries, LatestQueriesSetup, LatestQueriesVerification> mock,
            string nisCode,
            string postcode)
        {
            return mock.Arrange(
                Produce.One(Generate.TblPostInfo
                    .Select(g => g
                        .WithNisCode(nisCode)
                        .WithPostcode(postcode))),
                (when, x) => when.LatestPostInfo(x));
        }

        public static IEnumerable<PostalInfoLatestItem> ArrangeLatestPostInfoForPartOfMunicipality(
            this Mocking<ILatestQueries, LatestQueriesSetup, LatestQueriesVerification> mock,
            string nisCode,
            string partName)
        {
            return mock.Arrange(
                Produce.One(Generate.TblPostInfo
                    .Select(g => g
                        .WithNisCode(nisCode)
                        .WithPostnaam(partName))),
                (when, x) => when.LatestPostInfo(x));
        }

        public static IEnumerable<StreetNameLatestItem> ArrangeLatestStraatnaam(
            this Mocking<ILatestQueries, LatestQueriesSetup, LatestQueriesVerification> mock,
            string nisCode,
            string gemeentenaam,
            string straatnaamId,
            string straatNaam,
            Guid streetNameId)
        {
            return mock.Arrange(
                Produce.One(Generate.TblStraatNaam
                    .Select(sn => sn
                        .WithStraatNaam(straatNaam)
                        .WithPersistentLocalId(straatnaamId)
                        .WithStraatNaamId(streetNameId)
                        .WithGemeenteId(nisCode))),
                (when, x) => when.LatestStraatnamenExistForGemeentenaam(x, gemeentenaam));
        }

        public static IEnumerable<AddressDetailItem> ArrangeLatestAdres(
            this Mocking<ILatestQueries, LatestQueriesSetup, LatestQueriesVerification> mock,
            MunicipalityLatestItem gemeente,
            StreetNameLatestItem straatNaam,
            string postcode,
            string huisnummer,
            string busnummer)
        {
            return mock.Arrange(
                Produce.One(Generate.TblHuisNummer
                    .Select(a => a
                        .WithStraatNaamId(straatNaam.StreetNameId)
                        .WithTblPostKanton(postcode)
                        .WithHuisNummer(huisnummer)
                        .WithBusnummer(busnummer))),
                (when, x) => when.LatestAdressenExist(x, straatNaam.PersistentLocalId, huisnummer, busnummer));
        }
    }
}
