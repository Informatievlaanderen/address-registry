using System;
using Be.Vlaanderen.Basisregisters.Crab;
using NodaTime;

namespace AddressRegistry.Importer.HouseNumber.Crab.Subaddress
{
    public static class CrabSubAddressMapperHelperExtensions
    {
        public static CrabTimestamp CorrectWhenTimeTravelingDelete(
            this CrabTimestamp timestamp,
            CrabModification? modification,
            DateTime startTime)
        {
            if (modification != CrabModification.Delete)
                return timestamp;

            var startTimeInstant = startTime.ToCrabInstant();

            return (Instant)timestamp < startTimeInstant
                // generated delete timestamp = start-time + buffer (to make sure it's after the start-times of all related records) 
                ? new CrabTimestamp(startTimeInstant.Plus(Duration.FromSeconds(5)))
                : timestamp;
        }

    }
}
