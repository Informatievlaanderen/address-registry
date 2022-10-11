namespace AddressRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class ApproveAddress
        {
            public static class ParentInvalidStatus
            {
                public const string Code = "AdresHuisnummerVoorgesteldGehistoreerdOfAfgekeurd";
                public const string Message = "Deze actie is enkel toegestaan op adressen waarbij het huisnummer de status 'inGebruik' heeft.";
            }
            public static class AddressInvalidStatus
            {
                public const string Code = "AdresGehistoreerdOfAfgekeurd";
                public const string Message = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld'.";
            }
        }
    }
}
