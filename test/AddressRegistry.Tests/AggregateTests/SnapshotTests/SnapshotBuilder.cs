namespace AddressRegistry.Tests.AggregateTests.SnapshotTests
{
    using System;
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
            long streamVersion,
            JsonSerializerSettings serializerSettings)
        {
            return new SnapshotContainer
            {
                Info = new SnapshotInfo { StreamVersion = streamVersion, Type = nameof(StreetNameSnapshot) },
                Data = JsonConvert.SerializeObject(snapshot, serializerSettings)
            };
        }

        public static StreetNameSnapshot WithMigratedNisCode(this StreetNameSnapshot snapshot, string nisCode)
        {
            return new StreetNameSnapshot(
                new StreetNamePersistentLocalId(snapshot.StreetNamePersistentLocalId),
                new MunicipalityId(snapshot.MunicipalityId),
                new NisCode(nisCode),
                snapshot.StreetNameStatus,
                snapshot.IsRemoved,
                ReadStreetNameAddresses(new StreetNamePersistentLocalId(snapshot.StreetNamePersistentLocalId), snapshot.Addresses));
        }

        public static StreetNameSnapshot WithMunicipalityId(this StreetNameSnapshot snapshot, Guid municipalityId)
        {
            return new StreetNameSnapshot(
                new StreetNamePersistentLocalId(snapshot.StreetNamePersistentLocalId),
                new MunicipalityId(municipalityId),
                new NisCode(snapshot.MigratedNisCode),
                snapshot.StreetNameStatus,
                snapshot.IsRemoved,
                ReadStreetNameAddresses(new StreetNamePersistentLocalId(snapshot.StreetNamePersistentLocalId), snapshot.Addresses));
        }

        public static StreetNameSnapshot WithAddress(
            this StreetNameSnapshot snapshot,
            AddressPersistentLocalId addressPersistentLocalId,
            AddressStatus addressStatus,
            PostalCode postalCode,
            HouseNumber houseNumber,
            BoxNumber? boxNumber,
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification,
            ExtendedWkbGeometry geometryPosition,
            AddressPersistentLocalId? parentAddressPersistentLocalId,
            AddressPersistentLocalId? mergedAddressPersistentLocalId,
            AddressStatus? desiredStatus,
            string eventHash,
            ProvenanceData provenanceData)
        {
            var addresses = ReadStreetNameAddresses(new StreetNamePersistentLocalId(snapshot.StreetNamePersistentLocalId), snapshot.Addresses);

            var newAddress = new StreetNameAddress(o => {});

            var parent = parentAddressPersistentLocalId is not null
                ? addresses.GetByPersistentLocalId(parentAddressPersistentLocalId)
                : null;

            newAddress.RestoreSnapshot(
                new StreetNamePersistentLocalId(snapshot.StreetNamePersistentLocalId),
                new AddressData(
                    addressPersistentLocalId,
                    addressStatus,
                    houseNumber,
                    boxNumber,
                    postalCode,
                    new AddressGeometry(geometryMethod, geometrySpecification, geometryPosition),
                    true,
                    false,
                    parentAddressPersistentLocalId is null ? null : addresses.GetByPersistentLocalId(parentAddressPersistentLocalId),
                    mergedAddressPersistentLocalId,
                    desiredStatus,
                    null,
                    eventHash,
                    provenanceData),
                parent);

            addresses.Add(newAddress);

            return new StreetNameSnapshot(
                new StreetNamePersistentLocalId(snapshot.StreetNamePersistentLocalId),
                new MunicipalityId(snapshot.MunicipalityId),
                string.IsNullOrEmpty(snapshot.MigratedNisCode) ? null : new NisCode(snapshot.MigratedNisCode),
                snapshot.StreetNameStatus,
                snapshot.IsRemoved,
                addresses);
        }

        private static StreetNameAddresses ReadStreetNameAddresses(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            IEnumerable<AddressData> addresses)
        {
            var newAddresses = new StreetNameAddresses();
            foreach (var snapshotAddress in addresses)
            {
                var address = new StreetNameAddress(o => { });

                var parent = snapshotAddress.ParentId is not null
                    ? newAddresses.GetByPersistentLocalId(new AddressPersistentLocalId(snapshotAddress.ParentId.Value))
                    : null;

                address.RestoreSnapshot(streetNamePersistentLocalId, snapshotAddress, parent);

                newAddresses.Add(address);
            }

            return newAddresses;
        }

        public static StreetNameSnapshot CreateDefaultSnapshot(StreetNamePersistentLocalId streetNamePersistentLocalId)
        {
            return new StreetNameSnapshot(
                streetNamePersistentLocalId,
                new MunicipalityId(Guid.NewGuid()),
                new NisCode("testniscode"),
                StreetNameStatus.Proposed,
                false,
                new StreetNameAddresses());
        }
    }
}
