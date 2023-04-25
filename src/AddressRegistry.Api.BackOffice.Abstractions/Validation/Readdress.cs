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
                public const string Code = "BrondAdresIdAfgekeurdGehistoreerd";
                public static string Message(string addressPuri) => $"Deze actie is enkel toegestaan op adressen met status 'voorgesteld' of 'inGebruik': {addressPuri}.";

                public static TicketError ToTicketError(string addressPuri) => new(Message(addressPuri), Code);
            }

            public static class AddressHasBoxNumber
            {
                public const string Code = "BronAdresIdBusnummer";
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
                public const string Code = "DoelHuisnummerOngeldigHuisnummerformaat";
                public static string Message(string houseNumber) => $"Ongeldig huisnummerformaat: {houseNumber}.";

                public static TicketError ToTicketError(string houseNumber) => new TicketError(Message(houseNumber), Code);
            }

            public static class DuplicateSourceAddressId
            {
                public const string Code = "BronAdresIdReedsInLijstHerAdresseer";
                public static string Message(string addressId) => $"Het bronAdresId zit meerdere keren in lijst van herAdresseer: {addressId}.";
            }

            public static class DuplicateDestinationHouseNumber
            {
                public const string Code = "DoelHuisnummerReedsInLijstHerAdresseer";
                public static string Message(string houseNumber) => $"Het doelHuisnummer zit meerdere keren in lijst van herAdresseer: {houseNumber}.";
            }

            public static class SourceAndDestinationAddressAreTheSame
            {
                public const string Code = "AdresBronAdresIdDoelHuisnummer";
                public static string Message(string addressId) => $"Het bron adres id verwijst reeds naar het doel busnummer: {addressId}.";

                public static TicketError ToTicketError(string addressId) => new TicketError(Message(addressId), Code);
            }

            public static class AddressToRetireIsNotSourceAddress
            {
                public const string Code = "OpgehevenAdresNietInLijstHerAdresseer";
                public const string Message = "Het op te heffen adres dient voor te komen in de lijst van herAdresseer.";
            }
        }
    }
}
