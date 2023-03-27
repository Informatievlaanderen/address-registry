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

            public static class CannotBeChanged
            {
                public const string Code = "AdresGehistoreerdOfAfgekeurd";
                public const string Message = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld' of 'inGebruik'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
