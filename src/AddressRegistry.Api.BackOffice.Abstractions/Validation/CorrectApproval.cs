using TicketingService.Abstractions;

namespace AddressRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class CorrectApproval
        {
            public static class AddressInvalidStatus
            {
                public const string Code = "AdresGehistoreerdOfAfgekeurd";
                public const string Message = "Deze actie is enkel toegestaan op adressen met status 'inGebruik'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class AddressIsNotOfficiallyAssigned
            {
                public const string Code = "AdresNietOfficeeltoegekend";
                public const string Message = "Deze actie is enkel toegestaan voor officieel toegekende adressen.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
