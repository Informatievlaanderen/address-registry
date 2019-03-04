namespace AddressRegistry.Importer.HouseNumber.Crab.Subaddress
{
    using System.Collections.Generic;
    using System.Linq;
    using Address.Commands.Crab;
    using AddressRegistry.Crab;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Be.Vlaanderen.Basisregisters.Crab;
    using HouseNumber = AddressRegistry.HouseNumber;

    public static class CrabSubaddressHouseNumberMapper
    {
        public static IEnumerable<ImportHouseNumberSubaddressFromCrab> Map(
            IEnumerable<tblHuisNummer_hist> huisnummerHist,
            int subadresId)
        {
            return huisnummerHist
                .Select(
                    huisNummerHist =>
                    {
                        MapLogging.Log(".");

                        return new ImportHouseNumberSubaddressFromCrab(
                            new CrabHouseNumberId(huisNummerHist.huisNummerId.Value),
                            new CrabSubaddressId(subadresId),
                            new CrabStreetNameId(huisNummerHist.straatNaamId.Value),
                            new HouseNumber(huisNummerHist.huisNummer),
                            new GrbNotation(huisNummerHist.GRBnotatie),
                            new CrabLifetime(huisNummerHist.beginDatum?.ToCrabLocalDateTime(), huisNummerHist.eindDatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(huisNummerHist.CrabTimestamp.ToCrabInstant()),
                            new CrabOperator(huisNummerHist.Operator),
                            CrabEnumMapper.Map(huisNummerHist.Bewerking),
                            CrabEnumMapper.Map(huisNummerHist.Organisatie));
                    });
        }

        public static ImportHouseNumberSubaddressFromCrab Map(
            tblHuisNummer huisNummer,
            int subadresId)
        {
            MapLogging.Log(".");

            return new ImportHouseNumberSubaddressFromCrab(
                new CrabHouseNumberId(huisNummer.huisNummerId),
                new CrabSubaddressId(subadresId),
                new CrabStreetNameId(huisNummer.straatNaamId),
                new HouseNumber(huisNummer.huisNummer),
                new GrbNotation(huisNummer.GRBnotatie),
                new CrabLifetime(huisNummer.beginDatum.ToCrabLocalDateTime(), huisNummer.eindDatum?.ToCrabLocalDateTime()),
                new CrabTimestamp(huisNummer.CrabTimestamp.ToCrabInstant()),
                new CrabOperator(huisNummer.Operator),
                CrabEnumMapper.Map(huisNummer.Bewerking),
                CrabEnumMapper.Map(huisNummer.Organisatie));
        }
    }
}
