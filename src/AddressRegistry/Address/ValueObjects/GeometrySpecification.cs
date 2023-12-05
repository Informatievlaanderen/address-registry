namespace AddressRegistry.Address
{
    using System;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public enum GeometrySpecification
    {
        Municipality = 1,
        Street = 2,
        Parcel = 3,
        Lot = 4,
        Stand = 5,
        Berth = 6,
        Building = 7,
        BuildingUnit = 8,
        Entry = 9,
        RoadSegment = 11,
    }
}
