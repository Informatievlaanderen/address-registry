namespace AddressRegistry.Api.Legacy.Convertors
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Consumer.Read.Municipality.Projections;

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
