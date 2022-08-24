namespace AddressRegistry.StreetName.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using DataStructures;
    using Newtonsoft.Json;

    [EventName("StreetNameSnapshot")]
    [EventSnapshot(nameof(SnapshotContainer) + "<StreetNameSnapshot>", typeof(SnapshotContainer))]
    [EventDescription("Snapshot of StreetName with Addresses")]
    public class StreetNameSnapshot
    {
        public int StreetNamePersistentLocalId { get; }

        public Guid MunicipalityId { get; }
        public string? MigratedNisCode { get; }
        public StreetNameStatus StreetNameStatus { get; }
        public bool IsRemoved { get; }

        public IEnumerable<AddressData> Addresses { get; }

        public StreetNameSnapshot(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            MunicipalityId municipalityId,
            NisCode? migratedNisCode,
            StreetNameStatus streetNameStatus,
            bool isRemoved,
            StreetNameAddresses addresses)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            MunicipalityId = municipalityId;
            MigratedNisCode = migratedNisCode is null ? (string?)null : migratedNisCode;
            StreetNameStatus = streetNameStatus;
            IsRemoved = isRemoved;
            Addresses = addresses.Select(x => new AddressData(x)).ToList();
        }

        [JsonConstructor]
        private StreetNameSnapshot(
            int streetNamePersistentLocalId,
            Guid municipalityId,
            string? migratedNisCode,
            StreetNameStatus streetNameStatus,
            bool isRemoved,
            IEnumerable<AddressData> addresses)
            : this(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                new MunicipalityId(municipalityId),
                string.IsNullOrEmpty(migratedNisCode) ? null : new NisCode(migratedNisCode),
                streetNameStatus,
                isRemoved,
                new StreetNameAddresses())
        {
            Addresses = addresses;
        }
    }
}
