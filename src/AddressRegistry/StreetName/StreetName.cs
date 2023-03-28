namespace AddressRegistry.StreetName
{
    using System.Collections.Generic;
    using System.Linq;
    using Address;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Commands;
    using DataStructures;
    using Events;
    using Exceptions;

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

        public void Readdress(
            IPersistentLocalIdGenerator persistentLocalIdGenerator,
            IEnumerable<ReaddressAddressItem> readdressItems,
            ReaddressExecutionContext executionContext)
        {
            GuardActiveStreetName();

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

                var houseNumberPersistentLocalId =
                    new AddressPersistentLocalId(persistentLocalIdGenerator.GenerateNextPersistentLocalId());

                ProposeAddress(
                    houseNumberPersistentLocalId,
                    sourceAddress.PostalCode,
                    MunicipalityId,
                    item.DestinationHouseNumber,
                    null,
                    sourceAddress.Geometry.GeometryMethod,
                    sourceAddress.Geometry.GeometrySpecification,
                    sourceAddress.Geometry.Geometry
                );

                proposedAddresses.Add(houseNumberPersistentLocalId);
                executionContext.AddressesAdded.Add((PersistentLocalId, houseNumberPersistentLocalId));
                executionContext.AddressesUpdated.Add((PersistentLocalId, houseNumberPersistentLocalId));

                readdressAddresses.Add(new ReaddressAddressData(
                    item.SourceAddressPersistentLocalId,
                    houseNumberPersistentLocalId,
                    sourceAddress.Status,
                    item.DestinationHouseNumber,
                    boxNumber: null,
                    sourceAddress.PostalCode,
                    sourceAddress.Geometry,
                    sourceAddress.IsOfficiallyAssigned,
                    parentAddressPersistentLocalId: null
                ));

                foreach (var boxNumberAddress in sourceAddress.Children.Where(x => x.IsActive))
                {
                    var boxNumberPersistentLocalId = new AddressPersistentLocalId(persistentLocalIdGenerator.GenerateNextPersistentLocalId());

                    ProposeAddress(
                        boxNumberPersistentLocalId,
                        sourceAddress.PostalCode,
                        MunicipalityId,
                        item.DestinationHouseNumber,
                        boxNumberAddress.BoxNumber,
                        boxNumberAddress.Geometry.GeometryMethod,
                        boxNumberAddress.Geometry.GeometrySpecification,
                        boxNumberAddress.Geometry.Geometry
                    );

                    proposedAddresses.Add(boxNumberPersistentLocalId);
                    executionContext.AddressesAdded.Add((PersistentLocalId, boxNumberPersistentLocalId));

                    readdressAddresses.Add(new ReaddressAddressData(
                        boxNumberAddress.AddressPersistentLocalId,
                        boxNumberPersistentLocalId,
                        boxNumberAddress.Status,
                        item.DestinationHouseNumber,
                        boxNumberAddress.BoxNumber,
                        sourceAddress.PostalCode,
                        boxNumberAddress.Geometry,
                        boxNumberAddress.IsOfficiallyAssigned,
                        houseNumberPersistentLocalId
                    ));

                    if (boxNumberAddress.Status == AddressStatus.Proposed)
                    {
                        RejectAddress(boxNumberAddress.AddressPersistentLocalId);
                    }
                    else if (boxNumberAddress.Status == AddressStatus.Current)
                    {
                        RetireAddress(boxNumberAddress.AddressPersistentLocalId);
                    }
                }
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
