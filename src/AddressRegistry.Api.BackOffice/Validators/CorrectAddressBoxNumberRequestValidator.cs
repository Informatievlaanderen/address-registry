namespace AddressRegistry.Api.BackOffice.Validators
{
    using Abstractions.Requests;
    using Abstractions.Validation;
    using FluentValidation;
    using StreetName;

    public class CorrectAddressBoxNumberRequestValidator : AbstractValidator<CorrectAddressBoxNumberRequest>
    {
        public CorrectAddressBoxNumberRequestValidator()
        {
            RuleFor(x => x.Busnummer)
                .Must(BoxNumber.HasValidFormat)
                .WithMessage(ValidationErrors.Common.BoxNumberInvalidFormat.Message)
                .WithErrorCode(ValidationErrors.Common.BoxNumberInvalidFormat.Code);
        }
    }
}
