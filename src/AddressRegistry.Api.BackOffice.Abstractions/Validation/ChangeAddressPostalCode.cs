namespace AddressRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class ChangeAddressPostalCode
        {
            public static class AddressHasBoxNumber
            {
                public const string Code = "AdresPostinfoGeenHuisnummer";
                public const string Message = "Het is niet mogelijk om de postcode van een busnummer te veranderen.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
