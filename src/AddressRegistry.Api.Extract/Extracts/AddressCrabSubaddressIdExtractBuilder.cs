namespace AddressRegistry.Api.Extract.Extracts
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Microsoft.EntityFrameworkCore;
    using Projections.Extract;
    using Projections.Extract.AddressCrabSubaddressIdExtract;

    public class AddressCrabSubaddressIdExtractBuilder
    {
        public static ExtractFile CreateAddressSubaddressIdFile(ExtractContext context)
        {
            var extractItems = context
               .AddressCrabSubaddressIdExtract
               .AsNoTracking()
               .Where(m => m.CrabSubaddressId.HasValue && m.PersistentLocalId.HasValue)
               .OrderBy(m => m.PersistentLocalId);

            return ExtractBuilder.CreateDbfFile<AddressCrabSubaddressIdDbaseRecord>(
                ExtractController.FileNameCrabSubadresId,
                new AddressCrabSubaddressIdDbaseSchema(),
                extractItems.Select(x => x.DbaseRecord),
                extractItems.Count);
        }
    }
}
