namespace AddressRegistry.Api.BackOffice.Validators
{
    using Address.Requests;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using FluentValidation;
    using Projections.Syndication;

    public class AddressProposeRequestValidator : AbstractValidator<AddressProposeRequest>
    {
        public AddressProposeRequestValidator(SyndicationContext syndicationContext)
        {
            RuleFor(x => x.StraatNaamId)
                .Must((_, straatNaamId) => OsloPuriValidator.TryParseIdentifier(straatNaamId, out var _))
                .WithMessage((_, straatNaamId) =>  ValidationErrorMessages.StreetNameInvalid(straatNaamId))
                .WithErrorCode(ValidationErrorCodes.StreetNameInvalid);

            RuleFor(x => x.PostInfoId)
                .MustAsync((_, postInfoId, ct) => PostalCodeValidator.PostalCodeExists(syndicationContext, postInfoId, ct))
                .WithMessage((_, postInfoId) => ValidationErrorMessages.PostalCodeDoesNotExist(postInfoId))
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
