namespace AddressRegistry.Api.BackOffice.Validators
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using Microsoft.EntityFrameworkCore;
    using Projections.Syndication;

    public static class PostalCodeValidator
    {
        public static async Task<bool> PostalCodeExists(SyndicationContext syndicationContext, string postInfoId, CancellationToken ct)
        {
            if (!OsloPuriValidator.TryParseIdentifier(postInfoId, out var identifier))
            {
                return false;
            }

            var result = await syndicationContext
                .PostalInfoLatestItems
                .FirstOrDefaultAsync(x => x.PostalCode == identifier, ct);

            return result is not null;
        }
    }
}
