using TicketingService.Abstractions;

namespace AddressRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class CorrectHouseNumber
        {
            public static class HouseNumberOfBoxNumberCannotBeChanged
            {
                public const string Code = "AdresCorrectieHuisnummermetBusnummer";
                public const string Message = "Te corrigeren huisnummer mag geen busnummer bevatten.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
