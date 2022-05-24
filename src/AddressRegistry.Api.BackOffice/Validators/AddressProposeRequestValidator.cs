namespace AddressRegistry.Api.BackOffice.Validators
{
    using Address.Requests;
    using FluentValidation;
    using Projections.Syndication;

    public class AddressProposeRequestValidator : AbstractValidator<AddressProposeRequest>
    {
        public AddressProposeRequestValidator(SyndicationContext syndicationContext)
        {
            RuleFor(x => x.StraatNaamId)
                .Must((_, straatNaamId) => OsloPuriValidator.TryParseIdentifier(straatNaamId, out var _))
                .WithMessage(ValidationErrorMessages.StreetNameInvalid)
                .WithErrorCode(ValidationErrorCodes.StreetNameInvalid);

            RuleFor(x => x.PostInfoId)
                .MustAsync((_, postInfoId, ct) => PostalCodeValidator.PostalCodeExists(syndicationContext, postInfoId, ct))
                .WithMessage(ValidationErrorMessages.PostalCodeDoesNotExist)
                .WithErrorCode(ValidationErrorCodes.PostalCodeDoesNotExist);

            RuleFor(x => x.Huisnummer)
                .Must(HouseNumberValidator.IsValid)
                .WithMessage(ValidationErrorMessages.HouseNumberInvalid)
                .WithErrorCode(ValidationErrorCodes.HouseNumberInvalid);

            RuleFor(x => x.Busnummer)
                .Must(BoxNumberValidator.IsValid)
                .WithMessage(ValidationErrorMessages.BoxNumberInvalid)
                .WithErrorCode(ValidationErrorCodes.BoxNumberInvalid);
        }
    }
}
