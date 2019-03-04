namespace AddressRegistry.Projections.Legacy.AddressDetail
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

        StreetNameOsloIdWasAssigned,

        StreetNamePrimaryLanguageWasCleared,
        StreetNamePrimaryLanguageWasCorrected,
        StreetNamePrimaryLanguageWasCorrectedToCleared,
        StreetNamePrimaryLanguageWasDefined,

        StreetNameSecondaryLanguageWasCleared,
        StreetNameSecondaryLanguageWasCorrected,
        StreetNameSecondaryLanguageWasCorrectedToCleared,
        StreetNameSecondaryLanguageWasDefined,

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
