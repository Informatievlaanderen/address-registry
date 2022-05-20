namespace AddressRegistry.Api.BackOffice.Validators
{
    using FluentValidation;
    using Microsoft.EntityFrameworkCore;
    using Projections.Syndication;

    public static class PostalCodeValidator
    {
        public static class ErrorCodes
        {
            public const string DoesNotExist = "AdresPostinfoNietGekendValidatie";
        }

        public static class ErrorMessages
        {
            public const string DoesNotExist = "Ongeldige postinfoId.";
        }

        public static IRuleBuilderOptions<T, string> PostalCodeExists<T>(
            this IRuleBuilder<T, string> ruleBuilder, SyndicationContext syndicationContext)
        {
            return ruleBuilder.MustAsync(async (_, postInfoId, _, cancel) =>
            {
                if (!UrlValidator.TryParseUrl(postInfoId, out var identifier))
                {
                    return false;
                }

                var result = await syndicationContext
                    .PostalInfoLatestItems
                    .FirstOrDefaultAsync(x => x.PostalCode == identifier, cancel);

                return result is not null;
            });
        }
    }
}
