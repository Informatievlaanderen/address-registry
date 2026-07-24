namespace AddressRegistry.Api.Oslo.Convertors
{
    using System;
    using AddressRegistry.Consumer.Read.Municipality.Projections;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;

    public static class MunicipalityLanguageExtensions
    {
        public static Taal ToTaal(this MunicipalityLanguage municipalityLanguage)
        {
            return municipalityLanguage switch
            {
                MunicipalityLanguage.Dutch => Taal.NL,
                MunicipalityLanguage.French => Taal.FR,
                MunicipalityLanguage.English => Taal.EN,
                MunicipalityLanguage.German => Taal.DE,
                _ => throw new ArgumentOutOfRangeException(nameof(municipalityLanguage), municipalityLanguage, null)
            };
        }

        public static Be.Vlaanderen.Basisregisters.GrAr.Oslo.Taal ToOsloTaal(this MunicipalityLanguage municipalityLanguage)
        {
            return municipalityLanguage switch
            {
                MunicipalityLanguage.Dutch => Be.Vlaanderen.Basisregisters.GrAr.Oslo.Taal.Nl,
                MunicipalityLanguage.French => Be.Vlaanderen.Basisregisters.GrAr.Oslo.Taal.Fr,
                MunicipalityLanguage.English => Be.Vlaanderen.Basisregisters.GrAr.Oslo.Taal.En,
                MunicipalityLanguage.German => Be.Vlaanderen.Basisregisters.GrAr.Oslo.Taal.De,
                _ => throw new ArgumentOutOfRangeException(nameof(municipalityLanguage), municipalityLanguage, null)
            };
        }
    }
}
