namespace AddressRegistry.Importer.HouseNumber.Console.Crab.HouseNumber
{
    using System.Collections.Generic;
    using System.Linq;
    using Address.Commands.Crab;
    using AddressRegistry.Crab;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Be.Vlaanderen.Basisregisters.Crab;
    using HouseNumber = AddressRegistry.HouseNumber;

    internal static class CrabHouseNumberMapper
    {
        public static IEnumerable<ImportHouseNumberFromCrab> Map(IEnumerable<tblHuisNummer_hist> huisnummersHist)
        {
            return huisnummersHist
                .Select(huisnummerHist =>
                {
                    MapLogging.Log(".");

                    return new ImportHouseNumberFromCrab(
                        new CrabHouseNumberId(huisnummerHist.huisNummerId.Value),
                        new CrabStreetNameId(huisnummerHist.straatNaamId.Value),
                        new HouseNumber(huisnummerHist.huisNummer),
                        new GrbNotation(huisnummerHist.GRBnotatie),
                        new CrabLifetime(huisnummerHist.beginDatum?.ToCrabLocalDateTime(), huisnummerHist.eindDatum?.ToCrabLocalDateTime()),
                        new CrabTimestamp(huisnummerHist.CrabTimestamp.ToCrabInstant()),
                        new CrabOperator(huisnummerHist.Operator), CrabEnumMapper.Map(huisnummerHist.Bewerking), CrabEnumMapper.Map(huisnummerHist.Organisatie));
                });
        }

        public static ImportHouseNumberFromCrab Map(tblHuisNummer huisnummer)
        {
            MapLogging.Log(".");

            return new ImportHouseNumberFromCrab(
                new CrabHouseNumberId(huisnummer.huisNummerId),
                new CrabStreetNameId(huisnummer.straatNaamId),
                new HouseNumber(huisnummer.huisNummer),
                new GrbNotation(huisnummer.GRBnotatie),
                new CrabLifetime(huisnummer.beginDatum.ToCrabLocalDateTime(), huisnummer.eindDatum?.ToCrabLocalDateTime()),
                new CrabTimestamp(huisnummer.CrabTimestamp.ToCrabInstant()),
                new CrabOperator(huisnummer.Operator), CrabEnumMapper.Map(huisnummer.Bewerking), CrabEnumMapper.Map(huisnummer.Organisatie));
        }
    }
}
