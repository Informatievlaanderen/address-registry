namespace AddressRegistry.Api.BackOffice.Validators
{
    using Abstractions.Requests;
    using Abstractions.Validation;
    using FluentValidation;

    public class AddressCorrectHouseNumberRequestValidator : AbstractValidator<AddressCorrectHouseNumberRequest>
    {
        public AddressCorrectHouseNumberRequestValidator()
        {
            RuleFor(x => x.Huisnummer)
                .Must(HouseNumberValidator.IsValid)
                .WithMessage(ValidationErrors.Common.HouseNumberInvalidFormat.Message)
                .WithErrorCode(ValidationErrors.Common.HouseNumberInvalidFormat.Code);
        }
    }
}
