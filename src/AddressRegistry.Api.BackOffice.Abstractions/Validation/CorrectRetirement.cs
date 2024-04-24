namespace AddressRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class CorrectRetirement
        {
            public static class ParentInvalidStatus
            {
                public const string Code = "AdresHuisnummerVoorgesteldAfgekeurdOfGehistoreerd";
                public const string Message = "Deze actie is enkel toegestaan op adressen waarbij het huisnummer de status 'inGebruik' heeft.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class AddressInvalidStatus
            {
                public const string Code = "AdresAfgekeurdOfVoorgesteld";
                public const string Message = "Deze actie is enkel toegestaan op adressen met status 'gehistoreerd'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
