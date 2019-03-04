namespace AddressRegistry.Importer.HouseNumber.Crab.HouseNumber
{
    using System.Collections.Generic;
    using System.Linq;
    using Address.Commands.Crab;
    using AddressRegistry.Crab;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Be.Vlaanderen.Basisregisters.Crab;

    internal static class CrabHouseNumberStatusMapper
    {
        public static IEnumerable<ImportHouseNumberStatusFromCrab> Map(IEnumerable<tblHuisnummerstatus_hist> huisnummerstatussesHist)
        {
            return
                huisnummerstatussesHist
                    .Select(huisnummerstatus =>
                    {
                        MapLogging.Log(".");

                        return new ImportHouseNumberStatusFromCrab(
                            new CrabHouseNumberStatusId(huisnummerstatus.huisnummerstatusid.Value),
                            new CrabHouseNumberId(huisnummerstatus.huisnummerid.Value),
                            CrabEnumMapper.Map(huisnummerstatus.Status),
                            new CrabLifetime(huisnummerstatus.begindatum?.ToCrabLocalDateTime(), huisnummerstatus.einddatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(huisnummerstatus.CrabTimestamp.ToCrabInstant()),
                            new CrabOperator(huisnummerstatus.Operator),
                            CrabEnumMapper.Map(huisnummerstatus.Bewerking),
                            CrabEnumMapper.Map(huisnummerstatus.Organisatie)
                        );
                    });
        }

        public static IEnumerable<ImportHouseNumberStatusFromCrab> Map(IEnumerable<tblHuisnummerstatus> huisnummerstatusses)
        {
            return
                huisnummerstatusses
                    .Select(huisnummerstatus =>
                    {
                        MapLogging.Log(".");

                        return new ImportHouseNumberStatusFromCrab(
                            new CrabHouseNumberStatusId(huisnummerstatus.huisnummerstatusid),
                            new CrabHouseNumberId(huisnummerstatus.huisnummerid),
                            CrabEnumMapper.Map(huisnummerstatus.Status),
                            new CrabLifetime(huisnummerstatus.begindatum.ToCrabLocalDateTime(), huisnummerstatus.einddatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(huisnummerstatus.CrabTimestamp.ToCrabInstant()),
                            new CrabOperator(huisnummerstatus.Operator),
                            CrabEnumMapper.Map(huisnummerstatus.Bewerking),
                            CrabEnumMapper.Map(huisnummerstatus.Organisatie)
                        );
                    });
        }
    }
}
