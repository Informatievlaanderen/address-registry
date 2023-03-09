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
    }
}
