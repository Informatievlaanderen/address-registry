namespace AddressRegistry.Api.Extract.Extracts
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Microsoft.EntityFrameworkCore;
    using Projections.Extract;
    using Projections.Extract.AddressCrabHouseNumberIdExtract;

    public static class AddressCrabHouseNumberIdExtractBuilder
    {
        public static ExtractFile CreateAddressCrabHouseNumberIdFile(ExtractContext context)
        {
            var extractItems = context
               .AddressCrabHouseNumberIdExtract
               .AsNoTracking()
               .Where(m => m.CrabHouseNumberId.HasValue && m.PersistentLocalId.HasValue)
               .OrderBy(m => m.PersistentLocalId);

            return ExtractBuilder.CreateDbfFile<AddressCrabHouseNumberIdDbaseRecord>(
                ExtractFileNames.CrabHouseNumberId,
                new AddressCrabHouseNumberIdDbaseSchema(),
                extractItems.Select(x => x.DbaseRecord!),
                extractItems.Count);
        }
    }
}
