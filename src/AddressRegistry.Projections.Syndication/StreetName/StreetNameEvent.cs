namespace AddressRegistry.Projections.Syndication.StreetName
{
    public enum StreetNameEvent
    {
        StreetNameBecameComplete,
        StreetNameBecameCurrent,
        StreetNameBecameIncomplete,

        StreetNameHomonymAdditionWasCleared,
        StreetNameHomonymAdditionWasCorrected,
        StreetNameHomonymAdditionWasCorrectedToCleared,
        StreetNameHomonymAdditionWasDefined,

        StreetNameNameWasCleared,
        StreetNameNameWasCorrected,
        StreetNameNameWasCorrectedToCleared,
        StreetNameWasNamed,

        StreetNamePersistentLocalIdentifierWasAssigned,

        StreetNameStatusWasRemoved,
        StreetNameStatusWasCorrectedToRemoved,
        StreetNameWasCorrectedToCurrent,
        StreetNameWasCorrectedToProposed,
        StreetNameWasCorrectedToRetired,
        StreetNameWasProposed,
        StreetNameWasRetired,

        StreetNameWasRegistered,
        StreetNameWasRemoved,
    }
}
