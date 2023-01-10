using TicketingService.Abstractions;

namespace AddressRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class CorrectBoxNumber
        {
            public static class HasNoBoxNumber
            {
                public const string Code = "AdresHuisnummerZonderBusnummer";
                public const string Message = "Het adres heeft geen te corrigeren busnummer.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
