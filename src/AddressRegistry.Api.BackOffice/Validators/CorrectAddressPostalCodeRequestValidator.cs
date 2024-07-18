namespace AddressRegistry.Api.BackOffice.Validators
{
    using Abstractions.Requests;
    using Abstractions.Validation;
    using Consumer.Read.Postal;
    using FluentValidation;

    public class CorrectAddressPostalCodeRequestValidator : AbstractValidator<CorrectAddressPostalCodeRequest>
    {
        public CorrectAddressPostalCodeRequestValidator(PostalConsumerContext postalConsumerContext)
        {
            RuleFor(x => x.PostInfoId)
                .MustAsync((_, postInfoId, ct) => PostalCodeValidator.PostalCodeExists(postalConsumerContext, postInfoId, ct))
                .WithMessage((_, postInfoId) => ValidationErrors.Common.PostalCode.DoesNotExist.Message(postInfoId))
                .WithErrorCode(ValidationErrors.Common.PostalCode.DoesNotExist.Code);
        }
    }
}
