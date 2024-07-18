namespace AddressRegistry.Api.BackOffice.Validators
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using Consumer.Read.Postal;
    using Microsoft.EntityFrameworkCore;

    public static class PostalCodeValidator
    {
        public static async Task<bool> PostalCodeExists(PostalConsumerContext postalConsumerContext, string postInfoId, CancellationToken ct)
        {
            if (!OsloPuriValidator.TryParseIdentifier(postInfoId, out var identifier))
            {
                return false;
            }

            var result = await postalConsumerContext
                .PostalLatestItems
                .FirstOrDefaultAsync(x => x.PostalCode == identifier, ct);

            return result is not null;
        }
    }
}
