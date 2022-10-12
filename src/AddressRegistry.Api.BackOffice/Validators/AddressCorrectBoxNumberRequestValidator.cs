namespace AddressRegistry.Api.BackOffice.Validators
{
    using Abstractions.Requests;
    using Abstractions.Validation;
    using FluentValidation;
    using StreetName;

    public class AddressCorrectBoxNumberRequestValidator : AbstractValidator<AddressCorrectBoxNumberRequest>
    {
        public AddressCorrectBoxNumberRequestValidator()
        {
            RuleFor(x => x.Busnummer)
                .Must(BoxNumber.HasValidFormat)
                .WithMessage(ValidationErrors.Common.BoxNumberInvalidFormat.Message)
                .WithErrorCode(ValidationErrors.Common.BoxNumberInvalidFormat.Code);
        }
    }
}
