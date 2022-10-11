namespace AddressRegistry.Api.BackOffice.Validators
{
    using Abstractions;
    using Abstractions.Requests;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using FluentValidation;
    using Projections.Syndication;
    using StreetName;

    public class AddressProposeRequestValidator : AbstractValidator<AddressProposeRequest>
    {
        public AddressProposeRequestValidator(SyndicationContext syndicationContext)
        {
            RuleFor(x => x.StraatNaamId)
                .Must((_, straatNaamId) => OsloPuriValidator.TryParseIdentifier(straatNaamId, out var _))
                .WithMessage((_, straatNaamId) =>  ValidationErrors.Common.StreetNameInvalid.Message(straatNaamId))
                .WithErrorCode(ValidationErrors.Common.StreetNameInvalid.Code);

            RuleFor(x => x.PostInfoId)
                .MustAsync((_, postInfoId, ct) => PostalCodeValidator.PostalCodeExists(syndicationContext, postInfoId, ct))
                .WithMessage((_, postInfoId) => ValidationErrorMessages.Address.PostalCodeDoesNotExist(postInfoId))
                .WithErrorCode(Deprecated.Address.PostalCodeDoesNotExist);

            RuleFor(x => x.Huisnummer)
                .Must(HouseNumberValidator.IsValid)
                .WithMessage(ValidationErrors.Common.HouseNumberInvalidFormat.Message)
                .WithErrorCode(ValidationErrors.Common.HouseNumberInvalidFormat.Code);

            RuleFor(x => x.Busnummer)
                .Must(BoxNumber.HasValidFormat)
                .WithMessage(ValidationErrors.Common.BoxNumberInvalidFormat.Message)
                .WithErrorCode(ValidationErrors.Common.BoxNumberInvalidFormat.Code);

            RuleFor(x => x.PositieGeometrieMethode)
                .Must(x => x is null or PositieGeometrieMethode.AangeduidDoorBeheerder or PositieGeometrieMethode.AfgeleidVanObject)
                .WithMessage(ValidationErrors.Common.PositionGeometryMethod.Invalid.Message)
                .WithErrorCode(ValidationErrors.Common.PositionGeometryMethod.Invalid.Code);

            RuleFor(x => x.PositieSpecificatie)
                .NotEmpty()
                .When(x => x.PositieGeometrieMethode == PositieGeometrieMethode.AangeduidDoorBeheerder)
                .DependentRules(() =>
                {
                    RuleFor(x => x.PositieSpecificatie)
                        .Must(PositionSpecificationValidator.IsValidWhenAppointedByAdministrator)
                        .When(x => x.PositieGeometrieMethode == PositieGeometrieMethode.AangeduidDoorBeheerder)
                        .WithMessage(ValidationErrors.Common.PositionSpecification.Invalid.Message)
                        .WithErrorCode(ValidationErrors.Common.PositionSpecification.Invalid.Code);
                })
                .WithMessage(ValidationErrors.Common.PositionSpecification.Required.Message)
                .WithErrorCode(ValidationErrors.Common.PositionSpecification.Required.Code);

            RuleFor(x => x.PositieSpecificatie)
                .Must(PositionSpecificationValidator.IsValidWhenDerivedFromObject)
                .When(x => x.PositieGeometrieMethode == PositieGeometrieMethode.AfgeleidVanObject)
                .WithMessage(ValidationErrors.Common.PositionSpecification.Invalid.Message)
                .WithErrorCode(ValidationErrors.Common.PositionSpecification.Invalid.Code);

            RuleFor(x => x.Positie)
                .NotEmpty()
                .When(x => x.PositieGeometrieMethode == PositieGeometrieMethode.AangeduidDoorBeheerder)
                .WithErrorCode(ValidationErrors.Common.Position.Required.Code)
                .WithMessage(ValidationErrors.Common.Position.Required.Message);

            RuleFor(x => x.Positie)
                .Must(gml => GmlPointValidator.IsValid(gml, GmlHelpers.CreateGmlReader()))
                .When(x => !string.IsNullOrEmpty(x.Positie))
                .WithErrorCode(ValidationErrors.Common.Position.InvalidFormat.Code)
                .WithMessage(ValidationErrors.Common.Position.InvalidFormat.Message);
        }
    }
}
