namespace AddressRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class CorrectRetirement
        {
            public static class ParentInvalidStatus
            {
                public const string Code = "AdresHuisnummerVoorgesteldAfgekeurdOfGehistoreerd";
                public const string Message = "Deze actie is enkel toegestaan op adressen waarbij het huisnummer de status ‘inGebruik’ heeft.";
            }

            public static class AddressIncorrectStatus
            {
                public const string Code = "AdresInGebruikOfGehistoreerd";
                public const string Message = "Deze actie is enkel toegestaan op een adres met status 'afgekeurd'.";
            }
        }
    }
}
