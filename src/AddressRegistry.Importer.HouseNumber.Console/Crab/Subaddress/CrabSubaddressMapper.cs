namespace AddressRegistry.Importer.HouseNumber.Console.Crab.Subaddress
{
    using System.Collections.Generic;
    using System.Linq;
    using Address.Commands.Crab;
    using AddressRegistry.Crab;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Be.Vlaanderen.Basisregisters.Crab;

    public static class CrabSubaddressMapper
    {
        public static IEnumerable<ImportSubaddressFromCrab> Map(IEnumerable<tblSubAdres_hist> subadresHist)
        {
            return subadresHist
                .Select(
                    subaddress =>
                    {
                        MapLogging.Log(".");

                        var crabModification = CrabEnumMapper.Map(subaddress.Bewerking);

                        return new ImportSubaddressFromCrab(
                            new CrabSubaddressId(subaddress.subAdresId.Value),
                            new CrabHouseNumberId(subaddress.huisNummerId.Value),
                            new BoxNumber(subaddress.subAdres),
                            new CrabBoxNumberType(subaddress.aardSubAdresCode),
                            new CrabLifetime(subaddress.beginDatum?.ToCrabLocalDateTime(), subaddress.eindDatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(subaddress.CrabTimestamp.ToCrabInstant())
                                .CorrectWhenTimeTravelingDelete(crabModification, subaddress.beginTijd.Value),
                            new CrabOperator(subaddress.Operator),
                            crabModification,
                            CrabEnumMapper.Map(subaddress.Organisatie));
                    });
        }

        public static ImportSubaddressFromCrab Map(tblSubAdres subaddress)
        {
            MapLogging.Log(".");

            return new ImportSubaddressFromCrab(
                new CrabSubaddressId(subaddress.subAdresId),
                new CrabHouseNumberId(subaddress.huisNummerId),
                new BoxNumber(subaddress.subAdres),
                new CrabBoxNumberType(subaddress.aardSubAdresCode),
                new CrabLifetime(subaddress.beginDatum.ToCrabLocalDateTime(), subaddress.eindDatum?.ToCrabLocalDateTime()),
                new CrabTimestamp(subaddress.CrabTimestamp.ToCrabInstant()),
                new CrabOperator(subaddress.Operator),
                CrabEnumMapper.Map(subaddress.Bewerking),
                CrabEnumMapper.Map(subaddress.Organisatie));
        }
    }
}
