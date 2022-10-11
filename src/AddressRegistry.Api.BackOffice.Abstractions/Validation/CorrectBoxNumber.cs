namespace AddressRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class CorrectBoxNumber
        {
            public static class BoxNumberIsRequired
            {
                public const string Code = "AdresHuisnummerValidatie";
                public const string Message = "Busnummer is een verplicht veld.";
            }

            public static class HasNoBoxNumber
            {
                public const string Code = "AdresHuisnummerZonderBusnummer";
                public const string Message = "Het adres heeft geen te corrigeren busnummer.";
            }
        }
    }
}
