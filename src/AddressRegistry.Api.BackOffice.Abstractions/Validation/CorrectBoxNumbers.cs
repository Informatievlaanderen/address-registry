namespace AddressRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class CorrectBoxNumbers
        {
            public static class EmptyBoxNumbers
            {
                public const string Code = "BusnummersLijstLeeg";
                public const string Message = "De lijst van busnummers kan niet leeg zijn.";
            }

            public static class AddressInvalidStatus
            {
                public const string Code = "AdresIdAfgekeurdGehistoreerd"; //TODO-pr
                public static string Message(int persistentLocalId) => $"Deze actie is enkel toegestaan op adressen met status 'voorgesteld' of 'inGebruik': {persistentLocalId}.";

                public static TicketError ToTicketError(int persistentLocalId) => new(Message(persistentLocalId), Code);
            }

            public static class MultipleHouseNumberAddresses
            {
                public const string Code = "VerschillendeHuisnummersNietToegestaan";
                public const string Message = "Lijst bevat verschillende huisnummers.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class AddressAlreadyExists
            {
                public const string Code = "AdresBestaandeHuisnummerBusnummerCombinatie"; //TODO-pr
                public static string Message(string houseNumber, string boxNumber) => $"Het huisnummer '{houseNumber}' in combinatie met busnummer '{boxNumber}' bestaat reeds voor de opgegeven straatnaam.";

                public static TicketError ToTicketError(string houseNumber, string boxNumber) => new(Message(houseNumber, boxNumber), Code);
            }

            public static class HasNoBoxNumber
            {
                public const string Code = "AdresHuisnummerZonderBusnummer"; //TODO-pr
                public static string MessageWithAdresId(string addressId) => $"Het adres '{addressId}' heeft geen te corrigeren busnummer.";

                public static TicketError ToTicketError(int persistentLocalId) => new(MessageWithAdresId(persistentLocalId.ToString()), Code);
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
