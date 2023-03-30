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
            IList<ReaddressAddressItem> readdressItems,
            ReaddressExecutionContext executionContext)
        {
            GuardActiveStreetName();

            var proposedAddresses = new List<AddressPersistentLocalId>();
            var readdressedAddresses = new List<ReaddressedAddressData>();

            foreach (var item in readdressItems)
            {
                var sourceAddress = StreetNameAddresses.GetNotRemovedByPersistentLocalId(item.SourceAddressPersistentLocalId);

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

                var destinationAddress = StreetNameAddresses.FindActiveParentByHouseNumber(item.DestinationHouseNumber);
                var destinationAddressPersistentLocalId = destinationAddress?.AddressPersistentLocalId;

                if (destinationAddress is null)
                {
                    destinationAddressPersistentLocalId = new AddressPersistentLocalId(persistentLocalIdGenerator.GenerateNextPersistentLocalId());

                    ProposeAddress(
                        destinationAddressPersistentLocalId,
                        sourceAddress.PostalCode,
                        MunicipalityId,
                        item.DestinationHouseNumber,
                        null,
                        sourceAddress.Geometry.GeometryMethod,
                        sourceAddress.Geometry.GeometrySpecification,
                        sourceAddress.Geometry.Geometry
                    );

                    proposedAddresses.Add(destinationAddressPersistentLocalId);
                    executionContext.AddressesAdded.Add((PersistentLocalId, destinationAddressPersistentLocalId));
                }
                else if(!readdressItems.Select(x => x.SourceAddressPersistentLocalId).Contains(destinationAddress.AddressPersistentLocalId))
                {
                    // If the destination address exists and it's never used as a source addres, then we can already reject or retire it's box number addresses.
                    foreach (var boxNumberAddress in destinationAddress.Children.Where(x => x.IsActive))
                    {
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

                executionContext.AddressesUpdated.Add((PersistentLocalId, destinationAddressPersistentLocalId!));

                readdressedAddresses.Add(new ReaddressedAddressData(
                    item.SourceAddressPersistentLocalId,
                    destinationAddressPersistentLocalId!,
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
                    if (proposedAddresses.Contains(boxNumberAddress.AddressPersistentLocalId))
                    {
                        continue;
                    }

                    var destinationBoxNumberAddress = destinationAddress?.Children.SingleOrDefault(x => x.BoxNumber == boxNumberAddress.BoxNumber);
                    var boxNumberPersistentLocalId = destinationBoxNumberAddress?.AddressPersistentLocalId
                        ?? new AddressPersistentLocalId(persistentLocalIdGenerator.GenerateNextPersistentLocalId());

                    if (destinationBoxNumberAddress is null)
                    {
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
                    }

                    readdressedAddresses.Add(new ReaddressedAddressData(
                        boxNumberAddress.AddressPersistentLocalId,
                        boxNumberPersistentLocalId,
                        boxNumberAddress.Status,
                        item.DestinationHouseNumber,
                        boxNumberAddress.BoxNumber,
                        sourceAddress.PostalCode,
                        boxNumberAddress.Geometry,
                        boxNumberAddress.IsOfficiallyAssigned,
                        destinationAddressPersistentLocalId
                    ));

                    // 11 -> 13 -> 15
                    // A     A1
                    // B      B
                    // When house number 13 is the current sourceAddress, then readdressedAddresses will contain boxNumberAddress 13B,
                    // because 11B's properies were readdressed,
                    // and in the case of house number 11, the current sourceAddress' house number is never used as a destination house number (it's only a source address, never a destination addres).
                    if (readdressedAddresses.Any(x => x.DestinationAddressPersistentLocalId == boxNumberAddress.AddressPersistentLocalId) ||
                        readdressItems.All(x => x.DestinationHouseNumber != sourceAddress.HouseNumber))
                    {
                        continue;
                    }

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
                readdressedAddresses));
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
