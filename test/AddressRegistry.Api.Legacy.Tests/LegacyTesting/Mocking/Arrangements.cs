namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting.Mocking
{
    using System.Collections.Generic;
    using AddressMatch.Matching;
    using Generate;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.StreetName;

    internal static class Arrangements
    {
        //    public static IGemeenteListResult ArrangeGemeente(this Mocking<IGetListGemeente, GetListGemeenteSetup, GetListGemeenteVerification> mock, string gemeentenaam, string nisCode = null)
        //    {
        //        var generator = from g in Generate.GemeenteListResult
        //                        select g.WithName(gemeentenaam);
        //        if (!string.IsNullOrEmpty(nisCode))
        //            generator = from g in generator
        //                        select g.WithVbrObjectID(nisCode);
        //        return mock.Arrange(generator,
        //                            (when, x) => when.GemeenteExists(x));
        //    }

        //    public static void ArrangePostInfoForPostCode(this Mocking<IVbrPostinfoService, VbrPostinfoServiceSetup, VbrPostinfoServiceVerification> mock, string postcode, string nisCode)
        //    {
        //        Generator<PostinfoDTOWithGemeente> postinfo = Generate.PostinfoDTOWithGemeente
        //            .Select(p => p.WithPostcode(postcode).WithNISCodes(new[] { nisCode }));

        //        mock.Arrange(postinfo,
        //            (when, x) => when.PostinfoExists(x));
        //    }

        //    public static void ArrangePostInfoForGemeentenaam(this Mocking<IVbrPostinfoService, VbrPostinfoServiceSetup, VbrPostinfoServiceVerification> mock, string gemeentenaam, string nisCode)
        //    {
        //        Generator<PostinfoDTOWithGemeente> postinfo = Generate.PostinfoDTOWithGemeente
        //            .Select(p => p.WithNISCodes(new[] { nisCode }));
        //        Generator<PostinfoDTOWithGemeente> generator = postinfo;

        //        if (!string.IsNullOrEmpty(gemeentenaam))
        //            generator = Produce.One(Generate.PostnaamDTO.Select(pn => pn.WithPostnaam(gemeentenaam)))
        //                .Then(postnamen => postinfo.Select(p => p.WithPostnamen(postnamen)));

        //        mock.Arrange(generator,
        //            (when, x) => when.PostinfoExists(x));
        //    }

        //    public static void ArrangeAdresMappingToHuisnummer(this Mocking<IFromVbrToCrabQueries, FromVbrToCrabQueriesSetup, FromVbrToCrabQueriesVerification> mock,
        //        int adresId, int huisnummerId)
        //    {
        //        mock.Arrange(
        //            Produce.One(Generate.AdresMappingQueryResult_ToHuisnummer.Select(m => m.WithCrabHuisnummerID(huisnummerId))),
        //            (when, x) => when.AdresMappingsToExistForAdresId(x, adresId));
        //    }

        //    public static IEnumerable<GemeenteLatestVersionWithObjectID> ArrangeGemeenteLatestVersion(this Mocking<ILatestVersionQueries, LatestVersionQueriesSetup, LatestVersionQueriesVerification> mock,
        //        string nisCode, string gemeentenaam, int version)
        //    {
        //        return mock.Arrange(Produce.One(Generate.GemeenteLatestVersionWithObjectID
        //            .Select(s => s.WithGemeentenaam(gemeentenaam)
        //                .WithVersion(version)
        //                .WithNisCode(nisCode))),
        //                (when, x) => when.GemeenteLatestVersionsExistsForNisCode(x, nisCode)
        //            );
        //    }

        //    public static IEnumerable<StraatnaamLatestVersionWithObjectID> ArrangeStraatnaamLatestVersion(this Mocking<ILatestVersionQueries, LatestVersionQueriesSetup, LatestVersionQueriesVerification> mock,
        //        string gemeentenaam, int straatnaamId, string straatnaam, int version)
        //    {
        //        return mock.Arrange(Produce.One(Generate.StraatnaamLatestVersionWithObjectID
        //            .Select(s => s.WithStraatnaam(straatnaam)
        //                .WithGemeentenaam(gemeentenaam)
        //                .WithStraatnaamID(straatnaamId)
        //                .WithVersion(version))),
        //                (when, x) => when.StraatnaamLatestVersionWithObjectIDsExistForGemeente(x, gemeentenaam)
        //            );
        //    }

        //    public static IEnumerable<AdresLatestVersionWithObjectID> ArrangeAdresLatestVersion(this Mocking<ILatestVersionQueries, LatestVersionQueriesSetup, LatestVersionQueriesVerification> mock,
        //        string gemeentenaam, int adresId, string straatnaam, string huisnummer, int version)
        //    {
        //        return mock.Arrange(Produce.One(Generate.AdresLatestVersionWithObjectID
        //            .Select(s => s.WithStraatnaam(straatnaam)
        //                .WithGemeentenaam(gemeentenaam)
        //                .WithHuisnummer(huisnummer)
        //                .WithVersion(version)
        //                .WithAdresID(adresId))),
        //                (when, x) => when.AdresLatestVersionWithObjectIDsExistFor(x, gemeentenaam, straatnaam, huisnummer, null)
        //            );
        //    }

        //    public static IEnumerable<LatestStraatnaam> ArrangeKadStraatnamen(this Mocking<IKadRrService, KadRrServiceSetup, KadRrServiceVerification> mock,
        //        string nisCode, string gemeentenaam, int straatnaamId, string straatnaam, string kadStraatcode)
        //    {
        //        return mock.Arrange(
        //        Produce.One(
        //            Generate.LatestStraatnaam
        //            .Select(s => s.WithVbrObjectID(straatnaamId)
        //            .WithNaam(straatnaam)
        //            .WithGemeentenaam(gemeentenaam)
        //            .WithNisCode(nisCode)
        //            )),
        //            (when, x) => when.StraatnamenExistForKadStraatcodeAndNISCode(x, kadStraatcode, nisCode));
        //    }

        //    public static LatestStraatnaam ArrangeRrStraatnaam(this Mocking<IKadRrService, KadRrServiceSetup, KadRrServiceVerification> mock,
        //        string postcode, string nisCode, string gemeentenaam, int straatnaamId, string straatnaam, string rrStraatcode)
        //    {
        //        return mock.Arrange(
        //            Generate.LatestStraatnaam
        //            .Select(s => s.WithVbrObjectID(straatnaamId)
        //            .WithNaam(straatnaam)
        //            .WithGemeentenaam(gemeentenaam)
        //            .WithNisCode(nisCode)
        //            ),
        //            (when, x) => when.StraatnaamExistsForRrStraatcodeAndPostcode(x, rrStraatcode, postcode));
        //    }

        public static IEnumerable<MunicipalityLatestItem> ArrangeLatestGemeente(this Mocking<ILatestQueries, LatestQueriesSetup, LatestQueriesVerification> mock, string nisCode, string gemeentenaam)
    {
        return mock.Arrange(
            Produce.One(Generate.tblGemeente.Select(
                g => g.WithNisGemeenteCode(nisCode)
                .WithGemeenteNaam(gemeentenaam))),
            (when, x) => when.LatestGemeentesExist(x));
    }

        public static IEnumerable<StreetNameLatestItem> ArrangeLatestStraatnaam(this Mocking<ILatestQueries, LatestQueriesSetup, LatestQueriesVerification> mock, string nisCode, string gemeentenaam, string straatnaamId, string straatNaam)
        {
            return mock.Arrange(Produce.One(Generate.tblStraatNaam
                .Select(sn => sn.WithStraatNaam(straatNaam)
                .WithOsloId(straatnaamId)
                .WithGemeenteId(nisCode))),
                (when, x) => when.LatestStraatnamenExistForGemeentenaam(x, gemeentenaam));
        }

        //    public static IEnumerable<LatestAdres> ArrangeLatestAdres(this Mocking<ILatestQueries, LatestQueriesSetup, LatestQueriesVerification> mock, LatestGemeente gemeente, LatestStraatnaam straatNaam,
        //        string postcode, string huisnummer, string busnummer)
        //    {
        //        return mock.Arrange(Produce.One(Generate.LatestAdres
        //            .Select(a => a.WithGemeente(gemeente)
        //                        .WithStraatnaam(straatNaam)
        //                        .WithPostcode(postcode)
        //                        .WithHuisnummer(huisnummer)
        //                        .WithBusnummer(busnummer))),
        //            (when, x) => when.LatestAdressenExist(x, straatNaam.VbrObjectID, huisnummer, busnummer));
        //    }
    }
}
