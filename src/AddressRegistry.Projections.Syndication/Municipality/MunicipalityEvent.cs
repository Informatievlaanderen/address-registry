namespace AddressRegistry.Projections.Syndication.Municipality
{
    public enum MunicipalityEvent
    {
        MunicipalityWasRegistered,
        MunicipalityWasNamed,
        MunicipalityNameWasCleared,
        MunicipalityNameWasCorrected,
        MunicipalityNameWasCorrectedToCleared,
        MunicipalityNisCodeWasDefined,
        MunicipalityNisCodeWasCorrected,
        MunicipalityOfficialLanguageWasAdded,
        MunicipalityOfficialLanguageWasRemoved,
        MunicipalityFacilityLanguageWasAdded,
        MunicipalityFacilityLanguageWasRemoved,
        MunicipalityBecameCurrent,
        MunicipalityWasCorrectedToCurrent,
        MunicipalityWasRetired,
        MunicipalityWasCorrectedToRetired,
    }
}
