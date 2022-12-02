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
            }

            public static class AddressRemoved
            {
                public const string Code = "VerwijderdAdres";
                public const string Message = "Verwijderd adres.";
            }

            public static class AddressNotFound
            {
                public const string Code = "AdresIsOnbestaand";
                public const string Message = "Onbestaand adres.";
            }

            public static class StreetNameInvalid
            {
                public const string Code = "AdresStraatnaamNietGekendValidatie";
                public static string Message(string streetNamePuri) => $"De straatnaam '{streetNamePuri}' is niet gekend in het straatnaamregister.";
            }

            public static class StreetNameStatusInvalidForCorrection
            {
                public const string Code = "AdresStraatnaamVoorgesteldOfInGebruik";
                public const string Message = "Deze actie is enkel toegestaan binnen straatnamen met status 'voorgesteld' of 'inGebruik'.";

            }

            public static class StreetNameIsNotActive
            {
                public const string Code = "AdresStraatnaamGehistoreerdOfAfgekeurd";
                public const string Message = "De straatnaam is gehistoreerd of afgekeurd.";

            }

            public static class HouseNumberInvalidFormat
            {
                public const string Code = "AdresOngeldigHuisnummerformaat";
                public const string Message = "Ongeldig huisnummerformaat.";
            }

            public static class BoxNumberInvalidFormat
            {
                public const string Code = "AdresOngeldigBusnummerformaat";
                public const string Message = "Ongeldig busnummerformaat.";
            }

            public static class Position
            {
                public static class Required
                {
                    public const string Code = "AdresPositieVerplicht";
                    public const string Message = "De positie is verplicht.";
                }

                public static class InvalidFormat
                {
                    public const string Code = "AdresPositieformaatValidatie";
                    public const string Message = "De positie is geen geldige gml-puntgeometrie.";
                }

                public static class CannotBeChanged
                {
                    public const string Code = "AdresGehistoreerdOfAfgekeurd";
                    public const string Message = "Deze actie is enkel toegestaan op adressen met status 'voorgesteld' of 'inGebruik'.";
                }
            }

            public static class PositionSpecification
            {
                public static class Required
                {
                    public const string Code = "AdresPositieSpecificatieVerplicht";
                    public const string Message = "PositieSpecificatie is verplicht.";
                }

                public static class Invalid
                {
                    public const string Code = "AdresPositieSpecificatieValidatie";
                    public const string Message = "Ongeldige positieSpecificatie.";
                }
            }

            public static class PositionGeometryMethod
            {
                public static class Invalid
                {
                    public const string Code = "AdresPositieGeometriemethodeValidatie";
                    public const string Message = "Ongeldige positieGeometrieMethode.";
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
                }

                public static class PostalCodeNotInMunicipality
                {
                    public const string Code = "AdresPostinfoNietInGemeente";
                    public const string Message = "De ingevoerde postcode wordt niet gebruikt binnen deze gemeente.";
                }
            }
        }
    }
}
