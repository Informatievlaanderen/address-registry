namespace AddressRegistry.Api.BackOffice.Validators
{
    using Abstractions;
    using Abstractions.Requests;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using FluentValidation;
    using Projections.Syndication;

    public class AddressCorrectPostalCodeRequestValidator : AbstractValidator<AddressCorrectPostalCodeRequest>
    {
        public AddressCorrectPostalCodeRequestValidator(SyndicationContext syndicationContext)
        {
            RuleFor(x => x.PostInfoId)
                .MustAsync((_, postInfoId, ct) => PostalCodeValidator.PostalCodeExists(syndicationContext, postInfoId, ct))
                .WithMessage((_, postInfoId) => ValidationErrors.Common.PostalCode.DoesNotExist.Message(postInfoId))
                .WithErrorCode(ValidationErrors.Common.PostalCode.DoesNotExist.Code);
        }
    }
}
