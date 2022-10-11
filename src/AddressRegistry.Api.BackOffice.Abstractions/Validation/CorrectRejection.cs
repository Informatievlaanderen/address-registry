namespace AddressRegistry.Api.BackOffice.Abstractions.Validation
{
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
        }
    }
}
