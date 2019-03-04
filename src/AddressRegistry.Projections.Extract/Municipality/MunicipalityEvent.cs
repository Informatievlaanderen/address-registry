namespace AddressRegistry.Projections.Extract.Municipality
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
        MunicipalityOfficialLanuageWasAdded,
        MunicipalityOfficialLanuageWasRemoved,
        MunicipalityFacilitiesLanuageWasAdded,
        MunicipalityFacilitiesLanuageWasRemoved,
    }
}
