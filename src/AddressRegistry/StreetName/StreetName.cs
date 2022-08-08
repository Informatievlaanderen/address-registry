namespace AddressRegistry.StreetName
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
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

        public void RetireStreetName()
        {
            if (Status != StreetNameStatus.Retired)
            {
                ApplyChange(new StreetNameWasRetired(PersistentLocalId));

                foreach (var address in StreetNameAddresses.ProposedStreetNameAddresses)
                {
                    address.RejectBecauseStreetNameWasRetired();
                }

                foreach (var address in StreetNameAddresses.CurrentStreetNameAddresses)
                {
                    address.RetireBecauseStreetNameWasRetired();
                }
            }
        }

        public void RemoveStreetName()
        {
            if (!IsRemoved)
            {
                ApplyChange(new StreetNameWasRemoved(PersistentLocalId));
            }
            //TODO: remove addresses?
        }

        public void MigrateAddress(
            AddressId addressId,
            AddressStreetNameId streetNameId,
            AddressPersistentLocalId addressPersistentLocalId,
            AddressStatus addressStatus,
            HouseNumber houseNumber,
            BoxNumber boxNumber,
            AddressGeometry geometry,
            bool? officiallyAssigned,
            PostalCode postalCode,
            bool isCompleted,
            bool isRemoved,
            AddressId? parentAddressId)
        {
            if (!RegionFilter.IsFlemishRegion(MigratedNisCode!))
            {
                return;
            }

            if (StreetNameAddresses.HasPersistentLocalId(addressPersistentLocalId))
            {
                throw new InvalidOperationException(
                    $"Cannot migrate address with id '{addressPersistentLocalId}' to streetname '{PersistentLocalId}'.");
            }

            AddressPersistentLocalId? parentPersistentLocalId = null;
            if (!EqualityComparer<Guid>.Default.Equals(parentAddressId ?? Guid.Empty, Guid.Empty))
            {
                var parent = StreetNameAddresses.FindParentByLegacyAddressId(parentAddressId ?? AddressId.Default);

                if (parent is null)
                {
                    throw new ParentAddressNotFoundException(PersistentLocalId, houseNumber);
                }

                parentPersistentLocalId = parent.AddressPersistentLocalId;
            }

            ApplyChange(new AddressWasMigratedToStreetName(
                PersistentLocalId,
                addressId,
                streetNameId,
                addressPersistentLocalId,
                addressStatus,
                houseNumber,
                boxNumber,
                geometry,
                officiallyAssigned ?? false,
                postalCode,
                isCompleted,
                isRemoved,
                parentPersistentLocalId));
        }

        public void ProposeAddress(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            PostalCode postalCode,
            MunicipalityId municipalityIdByPostalCode,
            HouseNumber houseNumber,
            BoxNumber? boxNumber)
        {
            GuardActiveStreetName(streetNamePersistentLocalId);

            if (municipalityIdByPostalCode != MunicipalityId)
            {
                throw new PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException();
            }

            var parent = StreetNameAddresses.FindActiveParentByHouseNumber(houseNumber);

            var isChild = boxNumber is not null;
            var isParent = !isChild;
            var parentFound = parent is not null;
            var parentNotFound = !parentFound;

            if (isParent && parentFound)
            {
                throw new ParentAddressAlreadyExistsException(houseNumber);
            }

            if (isChild && parentNotFound)
            {
                throw new ParentAddressNotFoundException(streetNamePersistentLocalId, houseNumber);
            }

            if (isChild && !parent.BoxNumberIsUnique(boxNumber!))
            {
                throw new DuplicateBoxNumberException(boxNumber!);
            }

            ApplyChange(new AddressWasProposedV2(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                parent?.AddressPersistentLocalId,
                postalCode,
                houseNumber,
                boxNumber));
        }

        public void ApproveAddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            var addressToApprove = StreetNameAddresses.FindByPersistentLocalId(addressPersistentLocalId);

            if (addressToApprove is null)
            {
                throw new AddressNotFoundException(addressPersistentLocalId);
            }

            addressToApprove.Approve();
        }

        public void RejectAddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            var addressToReject = StreetNameAddresses.FindByPersistentLocalId(addressPersistentLocalId);

            if (addressToReject is null)
            {
                throw new AddressNotFoundException(addressPersistentLocalId);
            }

            addressToReject.Reject();
        }

        public void DeregulateAddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            var addressToDeregulate = StreetNameAddresses.FindByPersistentLocalId(addressPersistentLocalId);

            if (addressToDeregulate is null)
            {
                throw new AddressNotFoundException(addressPersistentLocalId);
            }

            addressToDeregulate.Deregulate();
        }

        public void RegularizeAddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            var addressToRegularize = StreetNameAddresses.FindByPersistentLocalId(addressPersistentLocalId);

            if (addressToRegularize is null)
            {
                throw new AddressNotFoundException(addressPersistentLocalId);
            }

            addressToRegularize.Regularize();
        }

        public void RetireAddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            var addressToRetire = StreetNameAddresses.FindByPersistentLocalId(addressPersistentLocalId);

            if (addressToRetire is null)
            {
                throw new AddressNotFoundException(addressPersistentLocalId);
            }

            addressToRetire.Retire();
        }

        private void GuardActiveStreetName(StreetNamePersistentLocalId streetNamePersistentLocalId)
        {
            if (IsRemoved)
            {
                throw new StreetNameIsRemovedException(streetNamePersistentLocalId);
            }

            if (!IsActive)
            {
                throw new StreetNameNotActiveException(streetNamePersistentLocalId);
            }
        }

        #region Metadata

        protected override void BeforeApplyChange(object @event)
        {
            _ = new EventMetadataContext(new Dictionary<string, object>());
            base.BeforeApplyChange(@event);
        }

        #endregion

        public string GetAddressHash(AddressPersistentLocalId addressPersistentLocalId)
        {
            var address = StreetNameAddresses.FindByPersistentLocalId(addressPersistentLocalId);
            if (address == null)
            {
                throw new AggregateSourceException($"Cannot find a address entity with id {addressPersistentLocalId}");
            }

            return address.LastEventHash;
        }

        public object TakeSnapshot()
        {
            return new StreetNameSnapshot(
                PersistentLocalId,
                MigratedNisCode,
                Status,
                IsRemoved,
                StreetNameAddresses);
        }

        public ISnapshotStrategy Strategy { get; }
    }
}
