namespace AddressRegistry.Importer.HouseNumber.Crab.HouseNumber
{
    using System.Collections.Generic;
    using System.Linq;
    using Address.Commands.Crab;
    using AddressRegistry.Crab;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Be.Vlaanderen.Basisregisters.Crab;

    internal static class CrabHouseNumberPositionMapper
    {
        public static IEnumerable<ImportHouseNumberPositionFromCrab> Map(IEnumerable<tblAdrespositie_hist> huisnummerPositiesHist)
        {
            return
                huisnummerPositiesHist
                    .Select(
                        s =>
                        {
                            MapLogging.Log(".");

                            return new ImportHouseNumberPositionFromCrab(
                                new CrabAddressPositionId(s.adrespositieid.Value),
                                new CrabHouseNumberId(s.adresid.Value),
                                new WkbGeometry(s.Geometrie.WKB),
                                new CrabAddressNature(s.aardAdres),
                                CrabEnumMapper.Map(s.HerkomstAdrespositie),
                                new CrabLifetime(s.beginDatum?.ToCrabLocalDateTime(), s.einddatum?.ToCrabLocalDateTime()),
                                new CrabTimestamp(s.CrabTimestamp.ToCrabInstant()),
                                new CrabOperator(s.Operator),
                                CrabEnumMapper.Map(s.Bewerking),
                                CrabEnumMapper.Map(s.Organisatie)
                            );
                        });
        }

        public static IEnumerable<ImportHouseNumberPositionFromCrab> Map(IEnumerable<tblAdrespositie> huisnummerPosities)
        {
            return
                huisnummerPosities
                    .Select(
                        adrespositie =>
                        {
                            MapLogging.Log(".");

                            return new ImportHouseNumberPositionFromCrab(
                                new CrabAddressPositionId(adrespositie.adrespositieid),
                                new CrabHouseNumberId(adrespositie.adresid),
                                new WkbGeometry(adrespositie.Geometrie.WKB),
                                new CrabAddressNature(adrespositie.aardAdres),
                                CrabEnumMapper.Map(adrespositie.HerkomstAdrespositie),
                                new CrabLifetime(adrespositie.beginDatum.ToCrabLocalDateTime(), adrespositie.einddatum?.ToCrabLocalDateTime()),
                                new CrabTimestamp(adrespositie.CrabTimestamp.ToCrabInstant()),
                                new CrabOperator(adrespositie.Operator),
                                CrabEnumMapper.Map(adrespositie.Bewerking),
                                CrabEnumMapper.Map(adrespositie.Organisatie)
                            );
                        });
        }
    }
}
