namespace AddressRegistry.Api.BackOffice.Validators
{
    using Abstractions.Requests;
    using Abstractions.Validation;
    using FluentValidation;

    public class CorrectAddressHouseNumberRequestValidator : AbstractValidator<CorrectAddressHouseNumberRequest>
    {
        public CorrectAddressHouseNumberRequestValidator(
            HouseNumberValidator houseNumberValidator)
        {
            RuleFor(x => x.Huisnummer)
                .Must(houseNumberValidator.Validate)
                .WithMessage(ValidationErrors.Common.HouseNumberInvalidFormat.Message)
                .WithErrorCode(ValidationErrors.Common.HouseNumberInvalidFormat.Code);
        }
    }
}
