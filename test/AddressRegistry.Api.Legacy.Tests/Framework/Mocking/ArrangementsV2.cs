namespace AddressRegistry.Api.Legacy.Tests.Framework.Mocking
{
    using System;
    using System.Collections.Generic;
    using Consumer.Read.Municipality.Projections;
    using Consumer.Read.StreetName.Projections;
    using Generate;
    using Legacy.AddressMatch.V2.Matching;
    using Projections.Legacy.AddressDetailV2;
    using Projections.Syndication.PostalInfo;

    internal static class ArrangementsV2
    {
        public static IEnumerable<MunicipalityLatestItem> ArrangeLatestGemeente(
            this Mocking<ILatestQueries, LatestQueriesV2Setup, LatestQueriesV2Verification> mock,
            string nisCode,
            string gemeentenaam)
        {
            return mock.Arrange(
                Produce.One(GenerateV2.TblGemeente
                    .Select(g => g
                        .WithNisGemeenteCode(nisCode)
                        .WithGemeenteNaam(gemeentenaam))),
                (when, x) => when.LatestGemeentesExist(x));
        }

        public static IEnumerable<MunicipalityLatestItem> ArrangeLatestGemeenteWithRandomNisCodes(
            this Mocking<ILatestQueries, LatestQueriesV2Setup, LatestQueriesV2Verification> mock,
            string gemeentenaam,
            int count)
        {
            return mock.Arrange(
                Produce.Exactly(count, GenerateV2.TblGemeente
                    .Select(g => g.WithGemeenteNaam(gemeentenaam))),
                (when, x) => when.LatestGemeentes(x));
        }

        public static IEnumerable<PostalInfoLatestItem> ArrangeLatestPostInfo(
            this Mocking<ILatestQueries, LatestQueriesV2Setup, LatestQueriesV2Verification> mock,
            string nisCode,
            string postcode)
        {
            return mock.Arrange(
                Produce.One(GenerateV2.TblPostInfo
                    .Select(g => g
                        .WithNisCode(nisCode)
                        .WithPostcode(postcode))),
                (when, x) => when.LatestPostInfo(x));
        }

        public static IEnumerable<PostalInfoLatestItem> ArrangeLatestPostInfoForPartOfMunicipality(
            this Mocking<ILatestQueries, LatestQueriesV2Setup, LatestQueriesV2Verification> mock,
            string nisCode,
            string partName)
        {
            return mock.Arrange(
                Produce.One(GenerateV2.TblPostInfo
                    .Select(g => g
                        .WithNisCode(nisCode)
                        .WithPostnaam(partName))),
                (when, x) => when.LatestPostInfo(x));
        }

        public static IEnumerable<StreetNameLatestItem> ArrangeLatestStraatnaam(
            this Mocking<ILatestQueries, LatestQueriesV2Setup, LatestQueriesV2Verification> mock,
            string nisCode,
            string gemeentenaam,
            int straatnaamId,
            string straatNaam,
            Guid streetNameId)
        {
            return mock.Arrange(
                Produce.One(GenerateV2.TblStraatNaam
                    .Select(sn => sn
                        .WithStraatNaam(straatNaam)
                        .WithPersistentLocalId(straatnaamId)
                        .WithGemeenteId(nisCode))),
                (when, x) => when.LatestStraatnamenExistForGemeentenaam(x, gemeentenaam));
        }

        public static IEnumerable<AddressDetailItemV2> ArrangeLatestAdres(
            this Mocking<ILatestQueries, LatestQueriesV2Setup, LatestQueriesV2Verification> mock,
            MunicipalityLatestItem gemeente,
            StreetNameLatestItem straatNaam,
            string postcode,
            string huisnummer,
            string busnummer)
        {
            return mock.Arrange(
                Produce.One(GenerateV2.TblHuisNummer
                    .Select(a => a
                        .WithStraatNaamId(straatNaam.PersistentLocalId)
                        .WithTblPostKanton(postcode)
                        .WithHuisNummer(huisnummer)
                        .WithBusnummer(busnummer))),
                (when, x) => when.LatestAdressenExist(x, straatNaam.PersistentLocalId, huisnummer, busnummer));
        }
    }
}
