namespace AddressRegistry.Api.BackOffice.Validators
{
    using Abstractions;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using FluentValidation;

    public class AddressCorrectPositionRequestValidator : AbstractValidator<AddressCorrectPositionRequest>
    {
        public AddressCorrectPositionRequestValidator()
        {
            RuleFor(x => x.PositieGeometrieMethode)
                .Must(x => x is PositieGeometrieMethode.AangeduidDoorBeheerder or PositieGeometrieMethode.AfgeleidVanObject)
                .WithMessage(ValidationErrorMessages.Address.GeometryMethodInvalid)
                .WithErrorCode(ValidationErrors.Address.GeometryMethodInvalid);

            RuleFor(x => x.PositieSpecificatie)
                .NotEmpty()
                .When(x => x.PositieGeometrieMethode == PositieGeometrieMethode.AangeduidDoorBeheerder)
                .DependentRules(() =>
                {
                    RuleFor(x => x.PositieSpecificatie)
                        .Must(PositionSpecificationValidator.IsValidWhenAppointedByAdministrator)
                        .When(x => x.PositieGeometrieMethode == PositieGeometrieMethode.AangeduidDoorBeheerder)
                        .WithMessage(ValidationErrorMessages.Address.PositionSpecificationInvalid)
                        .WithErrorCode(ValidationErrors.Address.PositionSpecificationInvalid);
                })
                .WithMessage(ValidationErrorMessages.Address.PositionSpecificationRequired)
                .WithErrorCode(ValidationErrors.Address.PositionSpecificationRequired);

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
