namespace AddressRegistry.StreetName
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Events;

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

            foreach (var address in StreetNameAddresses.ProposedStreetNameAddressesFromMunicipalityMerger.Where(x => x.IsHouseNumberAddress))
            {
                address.Approve();
            }

            foreach (var address in StreetNameAddresses.ProposedStreetNameAddressesFromMunicipalityMerger.Where(x => x.IsBoxNumberAddress))
            {
                address.Approve();
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

        public void RejectStreetNameBecauseOfMunicipalityMerger(ICollection<StreetName> newStreetNames)
        {
            if (Status == StreetNameStatus.Rejected)
            {
                return;
            }

            foreach (var address in StreetNameAddresses.ProposedStreetNameAddresses.Where(address => address.IsBoxNumberAddress))
            {
                Reject(address);
            }
            foreach (var address in StreetNameAddresses.CurrentStreetNameAddresses.Where(address => address.IsBoxNumberAddress))
            {
                Retire(address);
            }

            foreach (var address in StreetNameAddresses.ProposedStreetNameAddresses.Where(address => address.IsHouseNumberAddress))
            {
                Reject(address);
            }
            foreach (var address in StreetNameAddresses.CurrentStreetNameAddresses.Where(address => address.IsHouseNumberAddress))
            {
                Retire(address);
            }

            ApplyChange(new StreetNameWasRejectedBecauseOfMunicipalityMerger(
                PersistentLocalId,
                newStreetNames.Select(x => x.PersistentLocalId)));

            void Reject(StreetNameAddress address) => address.RejectBecauseOfMunicipalityMerger();
            void Retire(StreetNameAddress address) => address.RetireBecauseOfMunicipalityMerger(newStreetNames);
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

        public void RetireStreetNameBecauseOfMunicipalityMerger(ICollection<StreetName> newStreetNames)
        {
            if (Status == StreetNameStatus.Retired)
            {
                return;
            }

            foreach (var address in StreetNameAddresses.ProposedStreetNameAddresses.Where(address => address.IsBoxNumberAddress))
            {
                Reject(address);
            }
            foreach (var address in StreetNameAddresses.CurrentStreetNameAddresses.Where(address => address.IsBoxNumberAddress))
            {
                Retire(address);
            }

            foreach (var address in StreetNameAddresses.ProposedStreetNameAddresses.Where(address => address.IsHouseNumberAddress))
            {
                Reject(address);
            }
            foreach (var address in StreetNameAddresses.CurrentStreetNameAddresses.Where(address => address.IsHouseNumberAddress))
            {
                Retire(address);
            }

            ApplyChange(new StreetNameWasRetiredBecauseOfMunicipalityMerger(
                PersistentLocalId,
                newStreetNames.Select(x => x.PersistentLocalId)));

            void Reject(StreetNameAddress address) => address.RejectBecauseOfMunicipalityMerger();
            void Retire(StreetNameAddress address) => address.RetireBecauseOfMunicipalityMerger(newStreetNames);
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

        public void RetireStreetNameBecauseOfRename(StreetNamePersistentLocalId destinationPersistentLocalId)
        {
            foreach (var address in StreetNameAddresses)
            {
                RejectOrRetireAddressForReaddress(address.AddressPersistentLocalId);
            }

            ApplyChange(new StreetNameWasRenamed(PersistentLocalId, destinationPersistentLocalId));
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
