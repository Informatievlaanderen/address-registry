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
        }

        public void RejectStreetName()
        {
            if (Status != StreetNameStatus.Rejected)
            {
                ApplyChange(new StreetNameWasRejected(PersistentLocalId));
            }
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
                ApplyChange(new StreetNameWasRemoved(PersistentLocalId));
            }
        }

        public void CorrectStreetNameNames(IDictionary<string,string> streetNameNames)
        {
            ApplyChange(new StreetNameNamesWereCorrected(
                PersistentLocalId,
                streetNameNames,
                StreetNameAddresses.Select(x => new AddressPersistentLocalId(x.AddressPersistentLocalId))));
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
