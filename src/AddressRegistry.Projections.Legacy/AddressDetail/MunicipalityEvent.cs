namespace AddressRegistry.Projections.Legacy.AddressDetail
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
        MunicipalityFacilitiesLanguageWasAdded,
        MunicipalityFacilitiesLanguageWasRemoved,
    }
}
