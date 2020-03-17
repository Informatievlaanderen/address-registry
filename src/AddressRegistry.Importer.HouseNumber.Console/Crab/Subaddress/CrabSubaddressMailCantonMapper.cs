namespace AddressRegistry.Importer.HouseNumber.Console.Crab.Subaddress
{
    using System.Collections.Generic;
    using System.Linq;
    using Address.Commands.Crab;
    using AddressRegistry.Crab;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Be.Vlaanderen.Basisregisters.Crab;

    public static class CrabSubaddressMailCantonMapper
    {
        public static IEnumerable<ImportSubaddressMailCantonFromCrab> GetCommandsFor(
            IEnumerable<tblHuisNummer_postKanton_hist> huisnummerPostKantonsHist,
            int subadresid)
        {
            return huisnummerPostKantonsHist
                .Select(huisNummerPostKanton =>
                {
                    MapLogging.Log(".");

                    return new ImportSubaddressMailCantonFromCrab(
                        new CrabHouseNumberMailCantonId(huisNummerPostKanton.huisNummer_postKanton_Id.Value),
                        new CrabHouseNumberId(huisNummerPostKanton.huisNummerId.Value),
                        new CrabSubaddressId(subadresid),
                        new CrabMailCantonId(huisNummerPostKanton.postKantonId.Value),
                        new CrabMailCantonCode(huisNummerPostKanton.PostkantonCode),
                        new CrabLifetime(huisNummerPostKanton.beginDatum?.ToCrabLocalDateTime(), huisNummerPostKanton.eindDatum?.ToCrabLocalDateTime()),
                        new CrabTimestamp(huisNummerPostKanton.CrabTimestamp.ToCrabInstant()),
                        new CrabOperator(huisNummerPostKanton.Operator),
                        CrabEnumMapper.Map(huisNummerPostKanton.Bewerking),
                        CrabEnumMapper.Map(huisNummerPostKanton.Organisatie));
                });
        }

        public static IEnumerable<ImportSubaddressMailCantonFromCrab> GetCommandsFor(
            IEnumerable<tblHuisNummer_postKanton> huisnummerPostKantons,
            int subadresid)
        {
            return huisnummerPostKantons
                .Select(huisNummerPostKanton =>
                {
                    MapLogging.Log(".");

                    return new ImportSubaddressMailCantonFromCrab(
                        new CrabHouseNumberMailCantonId(huisNummerPostKanton.huisNummer_postKanton_Id),
                        new CrabHouseNumberId(huisNummerPostKanton.huisNummerId),
                        new CrabSubaddressId(subadresid),
                        new CrabMailCantonId(huisNummerPostKanton.postKantonId),
                        new CrabMailCantonCode(huisNummerPostKanton.PostkantonCode),
                        new CrabLifetime(huisNummerPostKanton.beginDatum.ToCrabLocalDateTime(), huisNummerPostKanton.eindDatum?.ToCrabLocalDateTime()),
                        new CrabTimestamp(huisNummerPostKanton.CrabTimestamp.ToCrabInstant()),
                        new CrabOperator(huisNummerPostKanton.Operator),
                        CrabEnumMapper.Map(huisNummerPostKanton.Bewerking),
                        CrabEnumMapper.Map(huisNummerPostKanton.Organisatie));
                });
        }
    }
}
