namespace AddressRegistry.Importer.HouseNumber.Console.Crab.HouseNumber
{
    using System.Collections.Generic;
    using System.Linq;
    using Address.Commands.Crab;
    using AddressRegistry.Crab;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Be.Vlaanderen.Basisregisters.Crab;

    public static class CrabHouseNumberMailCantonMapper
    {
        public static IEnumerable<ImportHouseNumberMailCantonFromCrab> Map(IEnumerable<tblHuisNummer_postKanton_hist> huisnummerPostKantonsHist)
        {
            return huisnummerPostKantonsHist
                .Select(huisNummerPostKanton =>
                {
                    MapLogging.Log(".");

                    return new ImportHouseNumberMailCantonFromCrab(
                        new CrabHouseNumberMailCantonId(huisNummerPostKanton.huisNummer_postKanton_Id.Value),
                        new CrabHouseNumberId(huisNummerPostKanton.huisNummerId.Value),
                        new CrabMailCantonId(huisNummerPostKanton.postKantonId.Value),
                        new CrabMailCantonCode(huisNummerPostKanton.PostkantonCode),
                        new CrabLifetime(huisNummerPostKanton.beginDatum?.ToCrabLocalDateTime(), huisNummerPostKanton.eindDatum?.ToCrabLocalDateTime()),
                        new CrabTimestamp(huisNummerPostKanton.CrabTimestamp.ToCrabInstant()),
                        new CrabOperator(huisNummerPostKanton.Operator),
                        CrabEnumMapper.Map(huisNummerPostKanton.Bewerking),
                        CrabEnumMapper.Map(huisNummerPostKanton.Organisatie));
                });
        }

        public static IEnumerable<ImportHouseNumberMailCantonFromCrab> Map(IEnumerable<tblHuisNummer_postKanton> huisnummerPostKantons)
        {
            return huisnummerPostKantons
                .Select(huisNummerPostKanton =>
                {
                    MapLogging.Log(".");

                    return new ImportHouseNumberMailCantonFromCrab(
                        new CrabHouseNumberMailCantonId(huisNummerPostKanton.huisNummer_postKanton_Id),
                        new CrabHouseNumberId(huisNummerPostKanton.huisNummerId),
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
