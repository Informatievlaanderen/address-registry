namespace AddressRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class RetireAddress
        {
            public static class AddressInvalidStatus
            {
                public const string Code = "AdresVoorgesteldOfAfgekeurd";
                public const string Message = "Deze actie is enkel toegestaan op adressen met status 'inGebruik'.";
            }
        }
    }
}
