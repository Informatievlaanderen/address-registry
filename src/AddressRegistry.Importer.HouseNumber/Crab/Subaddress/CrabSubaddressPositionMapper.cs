namespace AddressRegistry.Importer.HouseNumber.Crab.Subaddress
{
    using System.Collections.Generic;
    using System.Linq;
    using Address.Commands.Crab;
    using AddressRegistry.Crab;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Be.Vlaanderen.Basisregisters.Crab;

    public static class CrabSubaddressPositionMapper
    {
        public static IEnumerable<ImportSubaddressPositionFromCrab> Map(IEnumerable<tblAdrespositie_hist> huisnummerPositiesHist)
        {
            return
                huisnummerPositiesHist
                    .Select(
                        s =>
                        {
                            MapLogging.Log(".");

                            return new ImportSubaddressPositionFromCrab(
                                new CrabAddressPositionId(s.adrespositieid.Value),
                                new CrabSubaddressId(s.adresid.Value),
                                new WkbGeometry(s.Geometrie.WKB),
                                new CrabAddressNature(s.aardAdres),
                                CrabEnumMapper.Map(s.HerkomstAdrespositie),
                                new CrabLifetime(s.beginDatum?.ToCrabLocalDateTime(), s.einddatum?.ToCrabLocalDateTime()),
                                new CrabTimestamp(s.CrabTimestamp.ToCrabInstant()),
                                new CrabOperator(s.Operator),
                                CrabEnumMapper.Map(s.Bewerking),
                                CrabEnumMapper.Map(s.Organisatie));
                        });
        }

        public static IEnumerable<ImportSubaddressPositionFromCrab> Map(IEnumerable<tblAdrespositie> huisnummerPosities)
        {
            return
                huisnummerPosities
                    .Select(
                        adrespositie =>
                        {
                            MapLogging.Log(".");

                            return new ImportSubaddressPositionFromCrab(
                                new CrabAddressPositionId(adrespositie.adrespositieid),
                                new CrabSubaddressId(adrespositie.adresid),
                                new WkbGeometry(adrespositie.Geometrie.WKB),
                                new CrabAddressNature(adrespositie.aardAdres),
                                CrabEnumMapper.Map(adrespositie.HerkomstAdrespositie),
                                new CrabLifetime(adrespositie.beginDatum.ToCrabLocalDateTime(), adrespositie.einddatum?.ToCrabLocalDateTime()),
                                new CrabTimestamp(adrespositie.CrabTimestamp.ToCrabInstant()),
                                new CrabOperator(adrespositie.Operator),
                                CrabEnumMapper.Map(adrespositie.Bewerking),
                                CrabEnumMapper.Map(adrespositie.Organisatie));
                        });
        }
    }
}
