namespace AddressRegistry.Projections.Syndication.Parcel
{
    public enum ParcelEvent
    {
        ParcelWasRegistered,
        ParcelWasRemoved,
        ParcelWasRecovered,
        ParcelWasRealized,
        ParcelWasRetired,
        ParcelWasCorrectedToRealized,
        ParcelWasCorrectedToRetired,
        ParcelAddressWasAttached,
        ParcelAddressWasDetached,
    }
}
