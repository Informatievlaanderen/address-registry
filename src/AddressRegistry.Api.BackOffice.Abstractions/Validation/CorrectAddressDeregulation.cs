using TicketingService.Abstractions;

namespace AddressRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class CorrectAddressDeregulation
        {
            public static class AddressInvalidStatus
            {
                public const string Code = "AdresGehistoreerdOfAfgekeurd";
                public const string Message = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld' of 'inGebruik'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
