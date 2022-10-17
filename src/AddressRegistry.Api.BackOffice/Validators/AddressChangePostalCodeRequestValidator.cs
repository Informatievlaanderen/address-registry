namespace AddressRegistry.Api.BackOffice.Validators
{
    using Abstractions.Requests;
    using Abstractions.Validation;
    using FluentValidation;
    using Projections.Syndication;

    public class AddressChangePostalCodeRequestValidator : AbstractValidator<AddressChangePostalCodeRequest>
    {
        public AddressChangePostalCodeRequestValidator(SyndicationContext syndicationContext)
        {
            RuleFor(x => x.PostInfoId)
                .MustAsync((_, postInfoId, ct) => PostalCodeValidator.PostalCodeExists(syndicationContext, postInfoId, ct))
                .WithMessage((_, postInfoId) => ValidationErrors.Common.PostalCode.DoesNotExist.Message(postInfoId))
                .WithErrorCode(ValidationErrors.Common.PostalCode.DoesNotExist.Code);
        }
    }
}
