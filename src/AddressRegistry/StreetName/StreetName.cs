namespace AddressRegistry.StreetName
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Address;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Commands;
    using DataStructures;
    using Events;
    using Exceptions;
    using Newtonsoft.Json;

    public partial class StreetName : AggregateRootEntity, ISnapshotable
    {
        public static StreetName Register(
            IStreetNameFactory streetNameFactory,
            StreetNameId streetNameId,
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            MunicipalityId municipalityId,
            NisCode nisCode,
            StreetNameStatus streetNameStatus)
        {
            var streetName = streetNameFactory.Create();
            streetName.ApplyChange(
                new MigratedStreetNameWasImported(
                    streetNameId,
                    streetNamePersistentLocalId,
                    municipalityId,
                    nisCode,
                    streetNameStatus));
            return streetName;
        }

        public static StreetName Register(
            IStreetNameFactory streetNameFactory,
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            MunicipalityId municipalityId,
            StreetNameStatus streetNameStatus)
        {
            var streetName = streetNameFactory.Create();
            streetName.ApplyChange(
                new StreetNameWasImported(
                    streetNamePersistentLocalId,
                    municipalityId,
                    streetNameStatus));
            return streetName;
        }

        public void ApproveStreetName()
        {
            if (Status != StreetNameStatus.Current)
            {
                ApplyChange(new StreetNameWasApproved(PersistentLocalId));
            }
        }

        public void RejectStreetName()
        {
            if (Status == StreetNameStatus.Rejected)
            {
                return;
            }

            RejectAddressesBecauseStreetNameWasRejected(
                StreetNameAddresses.ProposedStreetNameAddresses.Where(address => address.IsBoxNumberAddress));
            RetireAddressesBecauseStreetNameWasRejected(
                StreetNameAddresses.CurrentStreetNameAddresses.Where(address => address.IsBoxNumberAddress));

            RejectAddressesBecauseStreetNameWasRejected(
                StreetNameAddresses.ProposedStreetNameAddresses.Where(address => address.IsHouseNumberAddress));
            RetireAddressesBecauseStreetNameWasRejected(
                StreetNameAddresses.CurrentStreetNameAddresses.Where(address => address.IsHouseNumberAddress));

            ApplyChange(new StreetNameWasRejected(PersistentLocalId));
        }

        public void RetireStreetName()
        {
            if (Status == StreetNameStatus.Retired)
            {
                return;
            }

            RejectAddressesBecauseStreetNameWasRetired(
                StreetNameAddresses.ProposedStreetNameAddresses.Where(address => address.IsBoxNumberAddress));
            RetireAddressesBecauseStreetNameWasRetired(
                StreetNameAddresses.CurrentStreetNameAddresses.Where(address => address.IsBoxNumberAddress));

            RejectAddressesBecauseStreetNameWasRetired(
                StreetNameAddresses.ProposedStreetNameAddresses.Where(address => address.IsHouseNumberAddress));
            RetireAddressesBecauseStreetNameWasRetired(
                StreetNameAddresses.CurrentStreetNameAddresses.Where(address => address.IsHouseNumberAddress));

            ApplyChange(new StreetNameWasRetired(PersistentLocalId));
        }

        public void RemoveStreetName()
        {
            if (!IsRemoved)
            {
                foreach (var address in StreetNameAddresses.Where(address => address.IsBoxNumberAddress))
                {
                    address.RemoveBecauseStreetNameWasRemoved();
                }

                foreach (var address in StreetNameAddresses.Where(address => address.IsHouseNumberAddress))
                {
                    address.RemoveBecauseStreetNameWasRemoved();
                }

                ApplyChange(new StreetNameWasRemoved(PersistentLocalId));
            }
        }

        public void CorrectStreetNameNames(IDictionary<string, string> streetNameNames)
        {
            ApplyChange(new StreetNameNamesWereCorrected(
                PersistentLocalId,
                streetNameNames,
                StreetNameAddresses
                    .Where(x => !x.IsRemoved)
                    .Select(x => new AddressPersistentLocalId(x.AddressPersistentLocalId))));
        }

        public void CorrectStreetNameHomonymAdditions(IDictionary<string, string> streetNameHomonymAdditions)
        {
            ApplyChange(new StreetNameHomonymAdditionsWereCorrected(
                PersistentLocalId,
                streetNameHomonymAdditions,
                StreetNameAddresses
                    .Where(x => !x.IsRemoved)
                    .Select(x => new AddressPersistentLocalId(x.AddressPersistentLocalId))));
        }

        public void RemoveStreetNameHomonymAdditions(IList<string> streetNameHomonymAdditionLanguages)
        {
            ApplyChange(new StreetNameHomonymAdditionsWereRemoved(
                PersistentLocalId,
                streetNameHomonymAdditionLanguages,
                StreetNameAddresses
                    .Where(x => !x.IsRemoved)
                    .Select(x => new AddressPersistentLocalId(x.AddressPersistentLocalId))));
        }

        public void CorrectStreetNameApproval()
        {
            if (Status != StreetNameStatus.Proposed)
            {
                ApplyChange(new StreetNameWasCorrectedFromApprovedToProposed(PersistentLocalId));
            }
        }

        public void CorrectStreetNameRejection()
        {
            if (Status != StreetNameStatus.Proposed)
            {
                ApplyChange(new StreetNameWasCorrectedFromRejectedToProposed(PersistentLocalId));
            }
        }

        public void CorrectStreetNameRetirement()
        {
            if (Status != StreetNameStatus.Current)
            {
                ApplyChange(new StreetNameWasCorrectedFromRetiredToCurrent(PersistentLocalId));
            }
        }

        [EventTags(EventTag.For.Edit, EventTag.For.Sync)]
        [EventName(EventName)]
        [EventDescription("Het adres werd geheradresseerd.")]
        public class StreetNameWasReaddressed : IStreetNameEvent
        {
            public const string EventName = "StreetNameWasReaddressed"; // BE CAREFUL CHANGING THIS!!

            [EventPropertyDescription("Objectidentificator van de straatnaam aan dewelke het adres is toegewezen.")]
            public int StreetNamePersistentLocalId { get; }

            [EventPropertyDescription("De voorgestelde adressen.")]
            public IReadOnlyList<int> ProposedAddressPersistentLocalIds { get; }
            [EventPropertyDescription("De heradresseerde adressen.")]
            public IReadOnlyList<ReaddressAddressData> ReaddressAddresses { get; }

            [EventPropertyDescription("Metadata bij het event.")]
            public ProvenanceData Provenance { get; private set; }

            public StreetNameWasReaddressed(
                StreetNamePersistentLocalId streetNamePersistentLocalId,
                List<AddressPersistentLocalId> proposedAddressPersistentLocalIds,
                List<ReaddressAddressData> readdressAddresses)
            {
                StreetNamePersistentLocalId = streetNamePersistentLocalId;
                ProposedAddressPersistentLocalIds = proposedAddressPersistentLocalIds.Select(x => (int) x).ToList();
                ReaddressAddresses = readdressAddresses;
            }

            [JsonConstructor]
            private StreetNameWasReaddressed(
                int streetNamePersistentLocalId,
                List<int> proposedAddressPersistentLocalIds,
                List<ReaddressAddressData> readdressAddresses,
                ProvenanceData provenance)
                : this(
                    new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                    proposedAddressPersistentLocalIds.Select(x => new AddressPersistentLocalId(x)).ToList(),
                    readdressAddresses)
                => ((ISetProvenance) this).SetProvenance(provenance.ToProvenance());

            void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

            public IEnumerable<string> GetHashFields()
            {
                var fields = Provenance.GetHashFields().ToList();
                fields.Add(StreetNamePersistentLocalId.ToString(CultureInfo.InvariantCulture));

                foreach (var item in ProposedAddressPersistentLocalIds)
                {
                    fields.Add(item.ToString());
                }

                foreach (var item in ReaddressAddresses)
                {
                    fields.Add(item.SourceAddressPersistentLocalId.ToString());
                    fields.Add(item.DestinationAddressPersistentLocalId.ToString());
                    fields.Add(item.DestinationHouseNumber);
                    fields.Add(item.SourcePostalCode);
                    fields.Add(item.SourceStatus.ToString());
                    fields.Add(item.SourceGeometryMethod.ToString());
                    fields.Add(item.SourceGeometrySpecification.ToString());
                    fields.Add(item.SourceExtendedWkbGeometry);
                    fields.Add(item.SourceIsOfficiallyAssigned.ToString());
                }

                return fields;
            }

            public string GetHash() => this.ToEventHash(EventName);
        }

        public void Readdress(
            IPersistentLocalIdGenerator persistentLocalIdGenerator,
            IEnumerable<ReaddressAddressItem> readdressItems,
            ReaddressExecutionContext executionContext)
        {
            GuardActiveStreetName(PersistentLocalId);

            // CASE 1:
            // From address_5 to non-existing address_6

            // Propose new address_6
            // Generate persistentLocalId
            // Call ProposeAddress
            // Copy attributes from address_5 to address_6
            // attributes: status, position, postalCode, officially assigned

            var proposedAddresses = new List<AddressPersistentLocalId>();
            var readdressAddresses = new List<ReaddressAddressData>();

            foreach (var item in readdressItems)
            {
                var sourceAddress =
                    StreetNameAddresses.GetNotRemovedByPersistentLocalId(item.SourceAddressPersistentLocalId);

                // TODO: question, does housenumber already exist?

                if (!sourceAddress.IsHouseNumberAddress)
                {
                    throw new AddressHasBoxNumberException(sourceAddress.AddressPersistentLocalId);
                }

                if (!sourceAddress.IsActive)
                {
                    throw new AddressHasInvalidStatusException(sourceAddress.AddressPersistentLocalId);
                }

                if (sourceAddress.PostalCode is null)
                {
                    throw new AddressHasNoPostalCodeException(sourceAddress.AddressPersistentLocalId);
                }

                var newPersistentLocalId =
                    new AddressPersistentLocalId(persistentLocalIdGenerator.GenerateNextPersistentLocalId());

                ProposeAddress(
                    newPersistentLocalId,
                    sourceAddress.PostalCode,
                    MunicipalityId,
                    item.DestinationHouseNumber,
                    null,
                    sourceAddress.Geometry.GeometryMethod,
                    sourceAddress.Geometry.GeometrySpecification,
                    sourceAddress.Geometry.Geometry
                );

                proposedAddresses.Add(newPersistentLocalId);
                executionContext.AddressesAdded.Add((PersistentLocalId, newPersistentLocalId));

                readdressAddresses.Add(new ReaddressAddressData(
                    item.SourceAddressPersistentLocalId,
                    newPersistentLocalId,
                    sourceAddress.Status,
                    item.DestinationHouseNumber,
                    sourceAddress.PostalCode,
                    sourceAddress.Geometry,
                    sourceAddress.IsOfficiallyAssigned
                ));
            }

            ApplyChange(new StreetNameWasReaddressed(
                PersistentLocalId,
                proposedAddresses,
                readdressAddresses));
        }

        private void RejectAddressesBecauseStreetNameWasRejected(IEnumerable<StreetNameAddress> addresses)
        {
            foreach (var address in addresses)
            {
                address.RejectBecauseStreetNameWasRejected();
            }
        }

        private void RetireAddressesBecauseStreetNameWasRejected(IEnumerable<StreetNameAddress> addresses)
        {
            foreach (var address in addresses)
            {
                address.RetireBecauseStreetNameWasRejected();
            }
        }

        private static void RejectAddressesBecauseStreetNameWasRetired(IEnumerable<StreetNameAddress> addresses)
        {
            foreach (var address in addresses)
            {
                address.RejectBecauseStreetNameWasRetired();
            }
        }

        private static void RetireAddressesBecauseStreetNameWasRetired(IEnumerable<StreetNameAddress> addresses)
        {
            foreach (var address in addresses)
            {
                address.RetireBecauseStreetNameWasRetired();
            }
        }

        #region Metadata

        protected override void BeforeApplyChange(object @event)
        {
            _ = new EventMetadataContext(new Dictionary<string, object>());
            base.BeforeApplyChange(@event);
        }

        #endregion

        public object TakeSnapshot()
        {
            return new StreetNameSnapshot(
                PersistentLocalId,
                MunicipalityId,
                MigratedNisCode,
                Status,
                IsRemoved,
                StreetNameAddresses);
        }

        public ISnapshotStrategy Strategy { get; }
    }
}
