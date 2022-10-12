namespace AddressRegistry.Api.BackOffice.Validators
{
    using Abstractions.Requests;
    using Abstractions.Validation;
    using FluentValidation;
    using StreetName;

    public class AddressCorrectHouseNumberRequestValidator : AbstractValidator<AddressCorrectHouseNumberRequest>
    {
        public AddressCorrectHouseNumberRequestValidator()
        {
            RuleFor(x => x.Huisnummer)
                .Must(HouseNumber.HasValidFormat)
                .WithMessage(ValidationErrors.Common.HouseNumberInvalidFormat.Message)
                .WithErrorCode(ValidationErrors.Common.HouseNumberInvalidFormat.Code);
        }
    }
}
