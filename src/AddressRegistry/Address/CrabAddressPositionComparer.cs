namespace AddressRegistry.Address
{
    using System;
    using Be.Vlaanderen.Basisregisters.Crab;
    using System.Collections.Generic;

    [Obsolete("This is a legacy class and should not be used anymore.")]
    public class CrabAddressPositionComparer : IComparer<CrabAddressPositionOrigin?>
    {
        private readonly Dictionary<CrabAddressPositionOrigin, int> _addressPositionQualityByOrigins = new Dictionary<CrabAddressPositionOrigin, int>
        {
            {CrabAddressPositionOrigin.ManualIndicationFromEntryOfBuilding, 1},
            {CrabAddressPositionOrigin.ManualIndicationFromStand, 2},
            {CrabAddressPositionOrigin.ManualIndicationFromBerth, 3},
            {CrabAddressPositionOrigin.ManualIndicationFromBuilding, 4},
            {CrabAddressPositionOrigin.ManualIndicationFromLot, 5},
            {CrabAddressPositionOrigin.ManualIndicationFromParcel, 6},
            {CrabAddressPositionOrigin.DerivedFromBuilding, 7},
            {CrabAddressPositionOrigin.DerivedFromParcelGrb, 8},
            {CrabAddressPositionOrigin.DerivedFromParcelCadastre, 9},
            {CrabAddressPositionOrigin.InterpolatedBasedOnAdjacentHouseNumbersBuilding, 10},
            {CrabAddressPositionOrigin.InterpolatedBasedOnAdjacentHouseNumbersParcelGrb, 11},
            {CrabAddressPositionOrigin.InterpolatedBasedOnAdjacentHouseNumbersParcelCadastre, 12},
            {CrabAddressPositionOrigin.InterpolatedBasedOnRoadConnection, 13},
            {CrabAddressPositionOrigin.DerivedFromStreet, 14},
            {CrabAddressPositionOrigin.DerivedFromMunicipality, 15},
            {CrabAddressPositionOrigin.ManualIndicationFromMailbox, 16},
            {CrabAddressPositionOrigin.ManualIndicationFromUtilityConnection, 17},
            {CrabAddressPositionOrigin.ManualIndicationFromAccessToTheRoad, 18}
        };

        public int Compare(
            CrabAddressPositionOrigin? x,
            CrabAddressPositionOrigin? y)
        {
            if (!y.HasValue && !x.HasValue)
                return 0;

            if (!y.HasValue)
                return 1;

            if (!x.HasValue)
                return -1;

            return _addressPositionQualityByOrigins[y.Value].CompareTo(_addressPositionQualityByOrigins[x.Value]);
        }
    }
}
