using TicketingService.Abstractions;

namespace AddressRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class CorrectBoxNumbers
        {
            //TODO-rik
            public static class EmptyBoxNumbers
            {
                public const string Code = "BusnummersLijstLeeg";
                public const string Message = "De lijst van busnummers kan niet leeg zijn.";
            }

            public static class DuplicateAddressId
            {
                public const string Code = "DubbeleAdressenNietToegestaan";
                public static string Message(string addressId) => $"Het adresId zit meerdere keren in lijst van busnummers: {addressId}.";
            }

            public static class DuplicateBoxNumber
            {
                public const string Code = "DubbeleBusnummersNietToegestaan";
                public static string Message(string boxNumber) => $"Het busnummer zit meerdere keren in lijst van busnummers: {boxNumber}.";
            }
        }
    }
}
