namespace AddressRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class CorrectRemoval
        {
            public static class ParentInvalidStatus
            {
                public const string Code = "AdresHuisnummerGehistoreerdOfAfgekeurd";
                public const string Message = "Deze actie is enkel toegestaan op adressen waarbij het huisnummer de status 'voorgesteld' of 'inGebruik' heeft.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
