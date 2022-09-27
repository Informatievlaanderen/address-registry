namespace AddressRegistry.Api.BackOffice.Validators
{
    using Abstractions;
    using Abstractions.Requests;
    using FluentValidation;
    using StreetName;

    public class AddressCorrectBoxNumberRequestValidator : AbstractValidator<AddressCorrectBoxNumberRequest>
    {
        public AddressCorrectBoxNumberRequestValidator()
        {
            RuleFor(x => x.Busnummer)
                .Must(BoxNumber.HasValidFormat)
                .WithMessage(ValidationErrorMessages.Address.BoxNumberInvalid)
                .WithErrorCode(ValidationErrors.Address.BoxNumberInvalid);
        }
    }
}
