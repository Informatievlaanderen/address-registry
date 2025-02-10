namespace AddressRegistry.Api.BackOffice.Validators
{
    using System.Linq;
    using Abstractions;
    using Abstractions.Requests;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using FluentValidation;
    using FluentValidation.Results;
    using StreetName;

    public class CorrectAddressBoxNumbersRequestValidator : AbstractValidator<CorrectAddressBoxNumbersRequest>
    {
        public CorrectAddressBoxNumbersRequestValidator(BackOfficeContext backOfficeContext)
        {
            RuleFor(x => x.Busnummers)
                .NotEmpty()
                .WithMessage(ValidationErrors.CorrectBoxNumbers.EmptyBoxNumbers.Message)
                .WithErrorCode(ValidationErrors.CorrectBoxNumbers.EmptyBoxNumbers.Code);

            RuleForEach(x => x.Busnummers)
                .Must((request, item) => request.Busnummers.Count(x => x.AdresId == item.AdresId) == 1)
                .WithMessage((_, x) => ValidationErrors.CorrectBoxNumbers.DuplicateAddressId.Message(x.AdresId))
                .WithErrorCode(ValidationErrors.CorrectBoxNumbers.DuplicateAddressId.Code);

            RuleForEach(x => x.Busnummers)
                .Must((request, item) => request.Busnummers.Count(x => x.Busnummer == item.Busnummer) == 1)
                .WithMessage((_, x) => ValidationErrors.CorrectBoxNumbers.DuplicateBoxNumber.Message(x.Busnummer))
                .WithErrorCode(ValidationErrors.CorrectBoxNumbers.DuplicateBoxNumber.Code);

            RuleForEach(x => x.Busnummers)
                .Must(x =>
                    OsloPuriValidator.TryParseIdentifier(x.AdresId, out var addressId)
                    && int.TryParse(addressId, out _))
                .WithMessage((_, x) => ValidationErrors.Common.AddressNotFound.MessageWithAdresId(x.AdresId))
                .WithErrorCode(ValidationErrors.Common.AddressNotFound.Code)

                .Must(x => BoxNumber.HasValidFormat(x.Busnummer))
                .WithMessage((_, x) => ValidationErrors.Common.BoxNumberInvalidFormat.MessageWithBoxNumber(x.Busnummer))
                .WithErrorCode(ValidationErrors.Common.BoxNumberInvalidFormat.Code)

                .DependentRules(() =>
                {
                    RuleFor(x => x.Busnummers)
                        .Custom((busnummers, context) =>
                        {
                            var parsedBusnummers = busnummers
                                .Select(x => new
                                {
                                    x.AdresId,
                                    PersistentLocalId = x.AdresId.AsIdentifier().Map(int.Parse).Value
                                })
                                .ToList();

                            var addressPersistentLocalIds = parsedBusnummers
                                .Select(x => x.PersistentLocalId)
                                .ToList();

                            var existingPersistentLocalIds = backOfficeContext.AddressPersistentIdStreetNamePersistentIds
                                .Where(address => addressPersistentLocalIds.Contains(address.AddressPersistentLocalId))
                                .Select(x => x.AddressPersistentLocalId)
                                .ToList();

                            var notExistingPersistentLocalIds = addressPersistentLocalIds.Except(existingPersistentLocalIds).ToList();
                            foreach(var addressPersistentLocalId in notExistingPersistentLocalIds)
                            {
                                var index = parsedBusnummers.FindIndex(x => x.PersistentLocalId == addressPersistentLocalId);
                                var adresId = parsedBusnummers[index].AdresId;

                                context.AddFailure(new ValidationFailure
                                {
                                    PropertyName = $"{nameof(CorrectAddressBoxNumbersRequest.Busnummers)}[{index}]",
                                    ErrorCode = ValidationErrors.Common.AddressNotFound.Code,
                                    ErrorMessage = ValidationErrors.Common.AddressNotFound.MessageWithAdresId(adresId)
                                });
                            }
                        });
                });
        }
    }
}
