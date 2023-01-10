using TicketingService.Abstractions;

namespace AddressRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class RejectAddress
        {
            public static class AddressInvalidStatus
            {
                public const string Code = "AdresGehistoreerdOfInGebruik";
                public const string Message = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
