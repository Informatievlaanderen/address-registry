namespace AddressRegistry.Api.BackOffice.Validators
{
    using System.Text.RegularExpressions;
    using Abstractions;
    using Abstractions.Requests;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using Consumer.Read.Postal;
    using FluentValidation;
    using StreetName;

    public class ProposeAddressRequestValidator : AbstractValidator<ProposeAddressRequest>
    {
        public ProposeAddressRequestValidator(
            StreetNameExistsValidator streetNameExistsValidator,
            PostalConsumerContext postalConsumerContext,
            HouseNumberValidator houseNumberValidator,
            BoxNumberValidator boxNumberValidator)
        {
            RuleFor(x => x.StraatNaamId)
                .MustAsync(async (straatNaamId, ct) =>
                    OsloPuriValidator.TryParseIdentifier(straatNaamId, out var _)
                    && await streetNameExistsValidator.Exists(straatNaamId, ct))
                .WithMessage((_, straatNaamId) =>  ValidationErrors.Common.StreetNameInvalid.Message(straatNaamId))
                .WithErrorCode(ValidationErrors.Common.StreetNameInvalid.Code);

            RuleFor(x => x.PostInfoId)
                .MustAsync((_, postInfoId, ct) => PostalCodeValidator.PostalCodeExists(postalConsumerContext, postInfoId, ct))
                .WithMessage((_, postInfoId) => ValidationErrors.Common.PostalCode.DoesNotExist.Message(postInfoId))
                .WithErrorCode(ValidationErrors.Common.PostalCode.DoesNotExist.Code);

            RuleFor(x => x.Huisnummer)
                .Must(houseNumberValidator.Validate)
                .WithMessage(ValidationErrors.Common.HouseNumberInvalidFormat.Message)
                .WithErrorCode(ValidationErrors.Common.HouseNumberInvalidFormat.Code);

            RuleFor(x => x.Busnummer)
                .Must(boxNumberValidator.Validate!)
                .When(x => !string.IsNullOrEmpty(x.Busnummer))
                .WithMessage(ValidationErrors.Common.BoxNumberInvalidFormat.Message)
                .WithErrorCode(ValidationErrors.Common.BoxNumberInvalidFormat.Code);

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
