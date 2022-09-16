namespace AddressRegistry.Api.BackOffice.Validators
{
    using Abstractions;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using FluentValidation;
    using Projections.Syndication;

    public class AddressCorrectHouseNumberRequestValidator : AbstractValidator<AddressCorrectHouseNumberRequest>
    {
        public AddressCorrectHouseNumberRequestValidator()
        {
            RuleFor(x => x.Huisnummer)
                .Must(HouseNumberValidator.IsValid)
                .WithMessage(ValidationErrorMessages.Address.HouseNumberInvalid)
                .WithErrorCode(ValidationErrors.Address.HouseNumberInvalid);
        }
    }
}
