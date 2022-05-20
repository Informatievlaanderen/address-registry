namespace AddressRegistry.Api.BackOffice.Validators
{
    using Address.Requests;
    using FluentValidation;
    using Projections.Syndication;

    public class AddressProposeRequestValidator : AbstractValidator<AddressProposeRequest>
    {
        public AddressProposeRequestValidator(SyndicationContext syndicationContext)
        {
            RuleFor(x => x.PostInfoId)
                .PostalCodeExists(syndicationContext)
                .WithMessage(PostalCodeValidator.ErrorMessages.DoesNotExist)
                .WithErrorCode(PostalCodeValidator.ErrorCodes.DoesNotExist);
        }
    }
}
