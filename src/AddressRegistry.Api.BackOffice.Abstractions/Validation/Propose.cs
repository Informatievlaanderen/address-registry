using TicketingService.Abstractions;

namespace AddressRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class Propose
        {
            public static class AddressInvalidStatus
            {
                public const string Code = "AdresGehistoreerdOfAfgekeurd";
                public const string Message = "Deze actie is enkel toegestaan op adressen met status 'inGebruik'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class AddressHouseNumberUnknown
            {
                public const string Code = "AdresActiefHuisNummerNietGekendValidatie";
                public static string Message(string streetNamePuri, string houseNumber)
                    => $"Er bestaat geen actief adres zonder busnummer voor straatnaam '{streetNamePuri}' en huisnummer '{houseNumber}'.";
            }

            public static class BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCode
            {
                public const string Code = "AdresPostinfoNietHetzelfdeAlsHuisnummer";
                public const string Message = "De ingevoerde postcode komt niet overeen met de postcode van het huisnummer.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
