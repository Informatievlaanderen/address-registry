namespace AddressRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class CorrectRejection
        {
            public static class ParentInvalidStatus
            {
                public const string Code = "AdresHuisnummerAfgekeurdOfGehistoreerd";
                public const string Message =
                    "Deze actie is enkel toegestaan op adressen waarbij het huisnummer de status 'voorgesteld' of 'inGebruik' heeft.";
            }

            public static class AddressInvalidStatus
            {
                public const string Code = "AdresInGebruikOfGehistoreerd";
                public const string Message = "Deze actie is enkel toegestaan op adressen met status 'afgekeurd'.";
            }

            public static class InconsistentHouseNumber
            {
                public const string Code = "AdresBusnummerHuisnummerInconsistent";
                public const string Message = "Deze actie is niet toegestaan op een busnummer wegens een inconsistent huisnummer.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class InconsistentPostalCode
            {
                public const string Code = "AdresBusnummerPostcodeInconsistent";
                public const string Message = "Deze actie is niet toegestaan op busnummers wegens een inconsistente postcode.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
