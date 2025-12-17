namespace AddressRegistry.Api.BackOffice.Validators
{
    using Abstractions.Requests;
    using Abstractions.Validation;
    using FluentValidation;
    using StreetName;

    public class CorrectAddressBoxNumberRequestValidator : AbstractValidator<CorrectAddressBoxNumberRequest>
    {
        public CorrectAddressBoxNumberRequestValidator(BoxNumberValidator boxNumberValidator)
        {
            RuleFor(x => x.Busnummer)
                .Must(boxNumberValidator.Validate)
                .WithMessage(ValidationErrors.Common.BoxNumberInvalidFormat.Message)
                .WithErrorCode(ValidationErrors.Common.BoxNumberInvalidFormat.Code);
        }
    }
}
