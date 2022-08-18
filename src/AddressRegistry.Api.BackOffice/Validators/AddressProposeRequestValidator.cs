namespace AddressRegistry.Api.BackOffice.Validators
{
    using Abstractions;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
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

            RuleFor(x => x.PositieSpecificatie)
                .NotEmpty()
                .When(x => x.PositieGeometrieMethode == PositieGeometrieMethode.AangeduidDoorBeheerder)
                .WithMessage(ValidationErrorMessages.PositionSpecificationRequired)
                .WithErrorCode(ValidationErrorCodes.PositionSpecificationRequired);

            RuleFor(x => x.PositieSpecificatie)
                .Must(x =>
                    x == PositieSpecificatie.Ingang ||
                    x == PositieSpecificatie.Perceel ||
                    x == PositieSpecificatie.Lot ||
                    x == PositieSpecificatie.Standplaats ||
                    x == PositieSpecificatie.Ligplaats)
                .When(x => x.PositieGeometrieMethode == PositieGeometrieMethode.AangeduidDoorBeheerder)
                .WithMessage(ValidationErrorMessages.PositionSpecificationInvalid)
                .WithErrorCode(ValidationErrorCodes.PositionSpecificationInvalid);

            RuleFor(x => x.PositieSpecificatie)
                .Must(x =>
                    x is null ||
                    x == PositieSpecificatie.Gemeente ||
                    x == PositieSpecificatie.Wegsegment)
                .When(x => x.PositieGeometrieMethode == PositieGeometrieMethode.AfgeleidVanObject)
                .WithMessage(ValidationErrorMessages.PositionSpecificationInvalid)
                .WithErrorCode(ValidationErrorCodes.PositionSpecificationInvalid);

            RuleFor(x => x.Positie)
                .NotEmpty()
                .When(x => x.PositieGeometrieMethode == PositieGeometrieMethode.AangeduidDoorBeheerder)
                .WithErrorCode(ValidationErrorCodes.PositionRequired)
                .WithMessage(ValidationErrorMessages.PositionRequired);

            RuleFor(x => x.Positie)
                .Must(gml => GmlPointValidator.IsValid(gml, GmlHelpers.CreateGmlReader()))
                .When(x => !string.IsNullOrEmpty(x.Positie))
                .WithErrorCode(ValidationErrorCodes.PositionInvalidFormat)
                .WithMessage(ValidationErrorMessages.PositionInvalidFormat);
        }
    }
}
