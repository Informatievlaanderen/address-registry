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
                .WithMessage((_, straatNaamId) =>  ValidationErrorMessages.StreetName.StreetNameInvalid(straatNaamId))
                .WithErrorCode(ValidationErrors.StreetName.StreetNameInvalid);

            RuleFor(x => x.PostInfoId)
                .MustAsync((_, postInfoId, ct) => PostalCodeValidator.PostalCodeExists(syndicationContext, postInfoId, ct))
                .WithMessage((_, postInfoId) => ValidationErrorMessages.Address.PostalCodeDoesNotExist(postInfoId))
                .WithErrorCode(ValidationErrors.Address.PostalCodeDoesNotExist);

            RuleFor(x => x.Huisnummer)
                .Must(HouseNumberValidator.IsValid)
                .WithMessage(ValidationErrorMessages.Address.HouseNumberInvalid)
                .WithErrorCode(ValidationErrors.Address.HouseNumberInvalid);

            RuleFor(x => x.Busnummer)
                .Must(BoxNumberValidator.IsValid)
                .WithMessage(ValidationErrorMessages.Address.BoxNumberInvalid)
                .WithErrorCode(ValidationErrors.Address.BoxNumberInvalid);

            RuleFor(x => x.PositieSpecificatie)
                .NotEmpty()
                .When(x => x.PositieGeometrieMethode == PositieGeometrieMethode.AangeduidDoorBeheerder)
                .WithMessage(ValidationErrorMessages.Address.PositionSpecificationRequired)
                .WithErrorCode(ValidationErrors.Address.PositionSpecificationRequired);

            RuleFor(x => x.PositieSpecificatie)
                .Must(PositionSpecificationValidator.IsValidWhenAppointedByAdministrator)
                .When(x => x.PositieGeometrieMethode == PositieGeometrieMethode.AangeduidDoorBeheerder)
                .WithMessage(ValidationErrorMessages.Address.PositionSpecificationInvalid)
                .WithErrorCode(ValidationErrors.Address.PositionSpecificationInvalid);

            RuleFor(x => x.PositieSpecificatie)
                .Must(PositionSpecificationValidator.IsValidWhenDerivedFromObject)
                .When(x => x.PositieGeometrieMethode == PositieGeometrieMethode.AfgeleidVanObject)
                .WithMessage(ValidationErrorMessages.Address.PositionSpecificationInvalid)
                .WithErrorCode(ValidationErrors.Address.PositionSpecificationInvalid);

            RuleFor(x => x.Positie)
                .NotEmpty()
                .When(x => x.PositieGeometrieMethode == PositieGeometrieMethode.AangeduidDoorBeheerder)
                .WithErrorCode(ValidationErrors.Address.PositionRequired)
                .WithMessage(ValidationErrorMessages.Address.PositionRequired);

            RuleFor(x => x.Positie)
                .Must(gml => GmlPointValidator.IsValid(gml, GmlHelpers.CreateGmlReader()))
                .When(x => !string.IsNullOrEmpty(x.Positie))
                .WithErrorCode(ValidationErrors.Address.PositionInvalidFormat)
                .WithMessage(ValidationErrorMessages.Address.PositionInvalidFormat);
        }
    }
}
