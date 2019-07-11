namespace AddressRegistry.Projections.Syndication.BuildingUnit
{
    public enum BuildingEvent
    {
        BuildingBecameComplete,
        BuildingBecameIncomplete,
        BuildingWasRegistered,
        BuildingWasRemoved,

        BuildingUnitAddressWasAttached,
        BuildingUnitAddressWasDetached,
        BuildingUnitBecameComplete,
        BuildingUnitBecameIncomplete,
        BuildingUnitPersistentLocalIdentifierWasAssigned,
        BuildingUnitPositionWasAppointedByAdministrator,
        BuildingUnitPositionWasCorrectedToAppointedByAdministrator,
        BuildingUnitPositionWasCorrectedToDerivedFromObject,
        BuildingUnitPositionWasDerivedFromObject,
        BuildingUnitStatusWasRemoved,
        BuildingUnitWasAdded,
        BuildingUnitWasAddedToRetiredBuilding,
        BuildingUnitWasCorrectedToNotRealized,
        BuildingUnitWasCorrectedToPlanned,
        BuildingUnitWasCorrectedToRealized,
        BuildingUnitWasCorrectedToRetired,
        BuildingUnitWasNotRealized,
        BuildingUnitWasNotRealizedByBuilding,
        BuildingUnitWasNotRealizedByParent,
        BuildingUnitWasPlanned,
        BuildingUnitWasReaddedByOtherUnitRemoval,
        BuildingUnitWasReaddressed,
        BuildingUnitWasRealized,
        BuildingUnitWasRemoved,
        BuildingUnitWasRetired,
        BuildingUnitWasRetiredByParent,
        CommonBuildingUnitWasAdded,
    }
}
