namespace AddressRegistry.Api.BackOffice.Abstractions.Validation
{
    using StreetName;
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class Readdress
        {
            public static class AddressInvalidStatus
            {
                public const string Code = "AdresAfgekeurdGehistoreerd";
                public static string Message(string addressPuri) => $"Deze actie is enkel toegestaan op adressen met status 'voorgesteld' of 'inGebruik': {addressPuri}.";

                public static TicketError ToTicketError(string addressPuri) => new(Message(addressPuri), Code);
            }

            public static class AddressHasBoxNumber
            {
                public const string Code = "AdresBusnummer";
                public static string Message(string addressPuri) => $"Deze actie is niet toegestaan op adressen met een busnummer: {addressPuri}.";

                public static TicketError ToTicketError(string addressPuri) => new(Message(addressPuri), Code);
            }

            public static class AddressHasNoPostalCode
            {
                public const string Code = "AdresPostcode";
                public static string Message(AddressPersistentLocalId addressId) => $"Het bron adres '{addressId}' heeft geen postcode.";

                public static TicketError ToTicketError(AddressPersistentLocalId addressId) => new(Message(addressId), Code);
            }

            public static class EmptyAddressesToReaddress
            {
                public const string Code = "HerAdresseerLijstLeeg";
                public const string Message = "De lijst van te heradresseren adressen kan niet leeg zijn.";
            }

            public static class AddressNotFound
            {
                public const string Code = "AdresIsOnbestaand";
                public static string Message(string addressId) => $"Onbestaand adres '{addressId}'.";
            }

            public static class HouseNumberInvalidFormat
            {
                public const string Code = "AdresOngeldigHuisnummerformaat";
                public static string Message(string houseNumber) => $"Ongeldig huisnummerformaat '{houseNumber}'.";
            }

            public static class DuplicateSourceAddressId
            {
                public const string Code = "AdresDuplicateBronAdresId";
                public static string Message(string addressId) => $"Duplicate bron adres id '{addressId}'.";
            }

            public static class DuplicateDestinationHouseNumber
            {
                public const string Code = "AdresDuplicateDoelHuisnummer";
                public static string Message(string houseNumber) => $"Duplicate doel huisnummer '{houseNumber}'.";
            }
        }
    }
}
