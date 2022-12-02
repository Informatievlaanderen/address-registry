namespace AddressRegistry.Api.BackOffice.Validators
{
    using Abstractions;
    using Abstractions.Requests;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using FluentValidation;

    public class AddressChangePositionRequestValidator : AbstractValidator<AddressChangePositionRequest>
    {
        public AddressChangePositionRequestValidator()
        {
            RuleFor(x => x.PositieGeometrieMethode)
                .Must(x => x is PositieGeometrieMethode.AangeduidDoorBeheerder or PositieGeometrieMethode.AfgeleidVanObject)
                .WithMessage(ValidationErrors.Common.PositionGeometryMethod.Invalid.Message)
                .WithErrorCode(ValidationErrors.Common.PositionGeometryMethod.Invalid.Code);

            RuleFor(x => x.PositieSpecificatie)
                .NotEmpty()
                .DependentRules(() =>
                {
                    RuleFor(x => x.PositieSpecificatie)
                        .Must(PositionSpecificationValidator.IsValidWhenAppointedByAdministrator)
                        .When(x => x.PositieGeometrieMethode == PositieGeometrieMethode.AangeduidDoorBeheerder)
                        .WithMessage(ValidationErrors.Common.PositionSpecification.Invalid.Message)
                        .WithErrorCode(ValidationErrors.Common.PositionSpecification.Invalid.Code);

                    RuleFor(x => x.PositieSpecificatie)
                        .Must(PositionSpecificationValidator.IsValidWhenDerivedFromObject)
                        .When(x => x.PositieGeometrieMethode == PositieGeometrieMethode.AfgeleidVanObject)
                        .WithMessage(ValidationErrors.Common.PositionSpecification.Invalid.Message)
                        .WithErrorCode(ValidationErrors.Common.PositionSpecification.Invalid.Code);
                })
                .WithMessage(ValidationErrors.Common.PositionSpecification.Required.Message)
                .WithErrorCode(ValidationErrors.Common.PositionSpecification.Required.Code);

            RuleFor(x => x.Positie)
                .NotEmpty()
                .DependentRules(() =>
                {
                    RuleFor(x => x.Positie)
                        .Must(gml => GmlPointValidator.IsValid(gml, GmlHelpers.CreateGmlReader()))
                        .WithErrorCode(ValidationErrors.Common.Position.InvalidFormat.Code)
                        .WithMessage(ValidationErrors.Common.Position.InvalidFormat.Message);
                })
                .WithErrorCode(ValidationErrors.Common.Position.Required.Code)
                .WithMessage(ValidationErrors.Common.Position.Required.Message);
        }
    }
}
