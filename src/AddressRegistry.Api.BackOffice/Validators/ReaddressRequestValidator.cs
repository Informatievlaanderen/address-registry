namespace AddressRegistry.Api.BackOffice.Validators
{
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using Abstractions.Requests;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using FluentValidation;
    using StreetName;

    public class ReaddressRequestValidator : AbstractValidator<ReaddressRequest>
    {
        public ReaddressRequestValidator(
            StreetNameExistsValidator streetNameExistsValidator,
            BackOfficeContext backOfficeContext)
        {
            RuleFor(x => x.DoelStraatnaamId)
                .MustAsync(async (straatNaamId, ct) =>
                    OsloPuriValidator.TryParseIdentifier(straatNaamId, out _)
                    && await streetNameExistsValidator.Exists(straatNaamId, ct))
                .WithMessage((_, straatNaamId) => ValidationErrors.Common.StreetNameInvalid.Message(straatNaamId))
                .WithErrorCode(ValidationErrors.Common.StreetNameInvalid.Code);

            RuleFor(x => x.HerAdresseer)
                .NotEmpty()
                .WithMessage(ValidationErrors.Readdress.EmptyAddressesToReaddress.Message)
                .WithErrorCode(ValidationErrors.Readdress.EmptyAddressesToReaddress.Code);

            RuleForEach(x => x.HerAdresseer)
                .MustAsync(async (x, ct) =>
                    OsloPuriValidator.TryParseIdentifier(x.BronAdresId, out var addressId)
                    && int.TryParse(addressId, out var addressPersistentLocalId)
                    && await backOfficeContext.AddressPersistentIdStreetNamePersistentIds.FindAsync(new object?[] { addressPersistentLocalId }, ct) is not null)
                .WithMessage((_, x) => ValidationErrors.Readdress.AddressNotFound.Message(x.BronAdresId))
                .WithErrorCode(ValidationErrors.Readdress.AddressNotFound.Code);

            RuleForEach(x => x.HerAdresseer)
                .Must(x => HouseNumber.HasValidFormat(x.DoelHuisnummer))
                .WithMessage((_, x) => ValidationErrors.Readdress.HouseNumberInvalidFormat.Message(x.DoelHuisnummer))
                .WithErrorCode(ValidationErrors.Readdress.HouseNumberInvalidFormat.Code);

            RuleFor(x => x)
                .Must(x => ValidateOpheffenAddressen(x.OpheffenAdressen, x.HerAdresseer))
                .WithMessage("OpheffenAdressen andere items.")
                .WithErrorCode("OpheffenAdressenAnders")
                .WithName(nameof(ReaddressRequest.OpheffenAdressen))
                .When(x => x.OpheffenAdressen is not null);
        }

        private static bool ValidateOpheffenAddressen(
            IEnumerable<string> opheffenAdressen,
            IEnumerable<AddressToReaddressItem> hernummerAdressen)
        {
            var sourceAddresses = hernummerAdressen.Select(x => x.BronAdresId).ToList();
            return opheffenAdressen.All(x => sourceAddresses.Contains(x));
        }
    }
}
