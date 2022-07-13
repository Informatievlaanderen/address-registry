namespace AddressRegistry.Tests.AggregateTests.SnapshotTests
{
    using System.Collections;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using StreetName;
    using StreetName.DataStructures;
    using StreetName.Events;

    public static class SnapshotBuilder
    {
        public static SnapshotContainer Build(
            this StreetNameSnapshot snapshot,
            long position,
            JsonSerializerSettings serializerSettings)
        {
            return new SnapshotContainer
            {
                Info = new SnapshotInfo { Position = position, Type = nameof(StreetNameSnapshot) },
                Data = JsonConvert.SerializeObject(snapshot, serializerSettings)
            };
        }

        public static StreetNameSnapshot WithMigratedNisCode(this StreetNameSnapshot snapshot, string nisCode)
        {
            return new StreetNameSnapshot(
                new StreetNamePersistentLocalId(snapshot.StreetNamePersistentLocalId),
                new NisCode(nisCode),
                snapshot.StreetNameStatus,
                snapshot.IsRemoved,
                ReaddStreetNameAddresses(new StreetNamePersistentLocalId(snapshot.StreetNamePersistentLocalId), snapshot.Addresses));
        }

        public static StreetNameSnapshot WithAddress(
            this StreetNameSnapshot snapshot,
            AddressPersistentLocalId addressPersistentLocalId,
            AddressStatus addressStatus,
            PostalCode postalCode,
            HouseNumber houseNumber,
            BoxNumber? boxNumber,
            AddressPersistentLocalId? parentAddressPersistentLocalId,
            string eventHash,
            ProvenanceData provenanceData)
        {
            var addresses = ReaddStreetNameAddresses(new StreetNamePersistentLocalId(snapshot.StreetNamePersistentLocalId), snapshot.Addresses);

            var newAddress = new StreetNameAddress(o => {});
            newAddress.RestoreSnapshot(
                new StreetNamePersistentLocalId(snapshot.StreetNamePersistentLocalId),
                new AddressData(
                addressPersistentLocalId,
                addressStatus,
                houseNumber,
                boxNumber,
                postalCode,
                null,
                true,
                false,
                parentAddressPersistentLocalId is null ? null : addresses.GetByPersistentLocalId(parentAddressPersistentLocalId),
                null,
                eventHash,
                provenanceData));

            if (parentAddressPersistentLocalId is not null)
            {
                newAddress.SetParent(addresses.GetByPersistentLocalId(parentAddressPersistentLocalId));
            }

            addresses.Add(newAddress);

            return new StreetNameSnapshot(
                new StreetNamePersistentLocalId(snapshot.StreetNamePersistentLocalId),
                string.IsNullOrEmpty(snapshot.MigratedNisCode) ? null : new NisCode(snapshot.MigratedNisCode),
                snapshot.StreetNameStatus,
                snapshot.IsRemoved,
                addresses);
        }

        private static StreetNameAddresses ReaddStreetNameAddresses(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            IEnumerable<AddressData> addresses)
        {
            var newAddresses = new StreetNameAddresses();
            foreach (var snapshotAddress in addresses)
            {
                var address = new StreetNameAddress(o => { });
                address.RestoreSnapshot(streetNamePersistentLocalId, snapshotAddress);
                newAddresses.Add(address);
            }

            return newAddresses;
        }

        public static StreetNameSnapshot CreateDefaultSnapshot(StreetNamePersistentLocalId streetNamePersistentLocalId)
        {
            return new StreetNameSnapshot(
                streetNamePersistentLocalId,
                null,
                StreetNameStatus.Proposed,
                false,
                new StreetNameAddresses());
        }
    }
}
