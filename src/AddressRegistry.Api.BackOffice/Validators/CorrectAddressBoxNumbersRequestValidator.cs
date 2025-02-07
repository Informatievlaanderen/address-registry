namespace AddressRegistry.Api.BackOffice.Validators
{
    using System;
    using System.Linq;
    using Abstractions;
    using Abstractions.Requests;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using FluentValidation;
    using StreetName;

    public class CorrectAddressBoxNumbersRequestValidator : AbstractValidator<CorrectAddressBoxNumbersRequest>
    {
        //TODO-rik update validation errors
        //TODO-rik add unit tests for API layer en Lambda

        public CorrectAddressBoxNumbersRequestValidator(BackOfficeContext backOfficeContext)
        {
            RuleFor(x => x.Busnummers)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(ValidationErrors.Common.BoxNumberInvalidFormat.Message)
                .WithErrorCode(ValidationErrors.Common.BoxNumberInvalidFormat.Code)

                .Must(busnummers => busnummers.Count == busnummers.Select(a => a.AdresId).Distinct().Count())
                .WithMessage(ValidationErrors.Common.BoxNumberInvalidFormat.Message)
                .WithErrorCode(ValidationErrors.Common.BoxNumberInvalidFormat.Code)

                .Must(busnummers => busnummers.Count == busnummers.Select(a => a.Busnummer).Distinct().Count())
                .WithMessage(ValidationErrors.Common.BoxNumberInvalidFormat.Message)
                .WithErrorCode(ValidationErrors.Common.BoxNumberInvalidFormat.Code);

            RuleForEach(x => x.Busnummers)
                .Must(x =>
                    OsloPuriValidator.TryParseIdentifier(x.AdresId, out var addressId)
                    && int.TryParse(addressId, out _))
                .WithMessage(ValidationErrors.Common.BoxNumberInvalidFormat.Message)
                .WithErrorCode(ValidationErrors.Common.BoxNumberInvalidFormat.Code)

                .Must(x => BoxNumber.HasValidFormat(x.Busnummer))
                .WithMessage(ValidationErrors.Common.BoxNumberInvalidFormat.Message)
                .WithErrorCode(ValidationErrors.Common.BoxNumberInvalidFormat.Code)

                .DependentRules(() =>
                {
                    RuleFor(x => x.Busnummers)
                        .Must(busnummers =>
                        {
                            //TODO-rik test of deze 1 keer wordt geinvoked, of per busnummer
                            var addressPersistentLocalIds = busnummers
                                .Select(x => x.AdresId.AsIdentifier().Map(int.Parse).Value)
                                .ToList();

                            var existingPersistentLocalIds = backOfficeContext.AddressPersistentIdStreetNamePersistentIds
                                .Where(address => addressPersistentLocalIds.Contains(address.AddressPersistentLocalId))
                                .Select(x => x.AddressPersistentLocalId)
                                .ToList();

                            return existingPersistentLocalIds.Count == addressPersistentLocalIds.Count;
                        });
                });
        }
    }
}
