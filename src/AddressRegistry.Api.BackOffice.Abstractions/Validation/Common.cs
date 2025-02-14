using TicketingService.Abstractions;

namespace AddressRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class Common
        {
            public static class AddressAlreadyExists
            {
                public const string Code = "AdresBestaandeHuisnummerBusnummerCombinatie";
                public const string Message = "Deze combinatie huisnummer-busnummer bestaat reeds voor de opgegeven straatnaam.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class AddressRemoved
            {
                public const string Code = "VerwijderdAdres";
                public const string Message = "Verwijderd adres.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class AddressRemovedWithId
            {
                public const string Code = "VerwijderdAdresId";
                public static string Message(int persistentLocalId) => $"Verwijderd adres '{persistentLocalId}'.";

                public static TicketError ToTicketError(int persistentLocalId) => new(Message(persistentLocalId), Code);
            }

            public static class AddressNotFound
            {
                public const string Code = "AdresIsOnbestaand";
                public const string Message = "Onbestaand adres.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class AddressNotFoundWithId
            {
                public const string Code = "AdresIdIsOnbestaand";
                public static string Message(string addressId) => $"Onbestaand adres '{addressId}'.";

                public static TicketError ToTicketError(string addressId) => new(Message(addressId), Code);
                public static TicketError ToTicketError(int persistentLocalId) => new(Message(persistentLocalId.ToString()), Code);
            }

            public static class AddressInconsistentHouseNumber
            {
                public const string Code = "AdresBusnummerHuisnummerInconsistent";
                public const string Message = "Deze actie is niet toegestaan op een busnummer wegens een inconsistent huisnummer.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class AddressInconsistentPostalCode
            {
                public const string Code = "AdresBusnummerPostcodeInconsistent";
                public const string Message = "Deze actie is niet toegestaan op een busnummer wegens een inconsistente postcode.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class ParentAddressRemoved
            {
                public const string Code = "VerwijderdHuisnummerAdres";
                public const string Message = "Verwijderd huisnummeradres.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class StreetNameInvalid
            {
                public const string Code = "AdresStraatnaamNietGekendValidatie";
                public static string Message(string streetNamePuri) => $"De straatnaam '{streetNamePuri}' is niet gekend in het straatnaamregister.";

                public static TicketError ToTicketError(string streetNamePuri) => new(Message(streetNamePuri), Code);
            }

            public static class StreetNameIsRemoved
            {
                public const string Code = "VerwijderdeStraatnaam";
                public const string Message = "Verwijderde straatnaam.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class StreetNameStatusInvalidForAction
            {
                public const string Code = "AdresStraatnaamVoorgesteldOfInGebruik";
                public const string Message = "Deze actie is enkel toegestaan binnen straatnamen met status 'voorgesteld' of 'inGebruik'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class StreetNameIsNotActive
            {
                public const string Code = "AdresStraatnaamGehistoreerdOfAfgekeurd";
                public const string Message = "De straatnaam is gehistoreerd of afgekeurd.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class HouseNumberInvalidFormat
            {
                public const string Code = "AdresOngeldigHuisnummerformaat";
                public const string Message = "Ongeldig huisnummerformaat.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class BoxNumberInvalidFormat
            {
                public const string Code = "AdresOngeldigBusnummerformaat";
                public const string Message = "Ongeldig busnummerformaat.";
                public static string MessageWithBoxNumber(string boxNumber) => $"Ongeldig busnummerformaat: {boxNumber}.";

                public static TicketError ToTicketError() => new(Message, Code);
                public static TicketError ToTicketError(string boxNumber) => new(MessageWithBoxNumber(boxNumber), Code);
            }

            public static class Position
            {
                public static class Required
                {
                    public const string Code = "AdresPositieVerplicht";
                    public const string Message = "De positie is verplicht.";

                    public static TicketError ToTicketError() => new(Message, Code);
                }

                public static class InvalidFormat
                {
                    public const string Code = "AdresPositieformaatValidatie";
                    public const string Message = "De positie is geen geldige gml-puntgeometrie.";

                    public static TicketError ToTicketError() => new(Message, Code);
                }

                public static class CannotBeChanged
                {
                    public const string Code = "AdresGehistoreerdOfAfgekeurd";
                    public const string Message = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld' of 'inGebruik'.";

                    public static TicketError ToTicketError() => new(Message, Code);
                }
            }

            public static class PositionSpecification
            {
                public static class Required
                {
                    public const string Code = "AdresPositieSpecificatieVerplicht";
                    public const string Message = "PositieSpecificatie is verplicht.";

                    public static TicketError ToTicketError() => new(Message, Code);
                }

                public static class Invalid
                {
                    public const string Code = "AdresPositieSpecificatieValidatie";
                    public const string Message = "Ongeldige positieSpecificatie.";

                    public static TicketError ToTicketError() => new(Message, Code);
                }
            }

            public static class PositionGeometryMethod
            {
                public static class Invalid
                {
                    public const string Code = "AdresPositieGeometriemethodeValidatie";
                    public const string Message = "Ongeldige positieGeometrieMethode.";

                    public static TicketError ToTicketError() => new(Message, Code);
                }
            }

            public static class PostalCode
            {
                public static class DoesNotExist
                {
                    public const string Code = "AdresPostinfoNietGekendValidatie";
                    public static string Message(string postInfoPuri)
                        => $"De postinfo '{postInfoPuri}' is niet gekend in het postinforegister.";
                }

                public static class CannotBeChanged
                {
                    public const string Code = "AdresGehistoreerdOfAfgekeurd";
                    public const string Message = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld' of 'inGebruik'.";

                    public static TicketError ToTicketError() => new(Message, Code);
                }

                public static class PostalCodeNotInMunicipality
                {
                    public const string Code = "AdresPostinfoNietInGemeente";
                    public const string Message = "De ingevoerde postcode wordt niet gebruikt binnen deze gemeente.";

                    public static TicketError ToTicketError() => new(Message, Code);
                }
            }
        }
    }
}
