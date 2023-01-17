namespace AddressRegistry.StreetName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        public void CorrectStreetNameApproval()
        {
            if (Status != StreetNameStatus.Proposed)
            {
                ApplyChange(new StreetNameWasCorrectedFromApprovedToProposed(PersistentLocalId));
            }
        }

        public void RejectStreetName()
        {
            if (Status != StreetNameStatus.Rejected)
            {
                ApplyChange(new StreetNameWasRejected(PersistentLocalId));
            }
        }

        public void CorrectStreetNameRejection()
        {
            if (Status != StreetNameStatus.Proposed)
            {
                ApplyChange(new StreetNameWasCorrectedFromRejectedToProposed(PersistentLocalId));
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

        public void CorrectStreetNameRetirement()
        {
            if (Status != StreetNameStatus.Current)
            {
                ApplyChange(new StreetNameWasCorrectedFromRetiredToCurrent(PersistentLocalId));
            }
        }

        public void CorrectStreetNameNames(IDictionary<string,string> streetNameNames)
        {
            ApplyChange(new StreetNameNamesWereCorrected(
                PersistentLocalId,
                streetNameNames,
                StreetNameAddresses.Select(x => new AddressPersistentLocalId(x.AddressPersistentLocalId))));
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

        public void RemoveStreetName()
        {
            if (!IsRemoved)
            {
                ApplyChange(new StreetNameWasRemoved(PersistentLocalId));
            }
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
            BoxNumber? boxNumber,
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification,
            ExtendedWkbGeometry geometryPosition)
        {
            GuardActiveStreetName(streetNamePersistentLocalId);

            if (StreetNameAddresses.HasPersistentLocalId(addressPersistentLocalId))
            {
                throw new AddressPersistentLocalIdAlreadyExistsException();
            }

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
                throw new AddressAlreadyExistsException(houseNumber, boxNumber!);
            }

            StreetNameAddress.GuardGeometry(geometryMethod, geometrySpecification);

            ApplyChange(new AddressWasProposedV2(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                parent?.AddressPersistentLocalId,
                postalCode,
                houseNumber,
                boxNumber,
                geometryMethod,
                geometrySpecification,
                geometryPosition));
        }

        public void ApproveAddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            StreetNameAddresses
                .GetNotRemovedByPersistentLocalId(addressPersistentLocalId)
                .Approve();
        }

        public void RejectAddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            StreetNameAddresses
                .GetNotRemovedByPersistentLocalId(addressPersistentLocalId)
                .Reject();
        }

        public void DeregulateAddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            StreetNameAddresses
                .GetNotRemovedByPersistentLocalId(addressPersistentLocalId)
                .Deregulate();
        }

        public void RegularizeAddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            StreetNameAddresses
                .GetNotRemovedByPersistentLocalId(addressPersistentLocalId)
                .Regularize();
        }

        public void RetireAddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            StreetNameAddresses
                .GetNotRemovedByPersistentLocalId(addressPersistentLocalId)
                .Retire();
        }

        public void ChangeAddressPosition(
            AddressPersistentLocalId addressPersistentLocalId,
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification,
            ExtendedWkbGeometry position)
        {
            GuardStreetNameStatusForChangeAndCorrection();

            StreetNameAddresses
                .GetNotRemovedByPersistentLocalId(addressPersistentLocalId)
                .ChangePosition(geometryMethod, geometrySpecification, position);
        }

        public void ChangeAddressPostalCode(AddressPersistentLocalId addressPersistentLocalId, PostalCode postalCode)
        {
            GuardStreetNameStatusForChangeAndCorrection();

            StreetNameAddresses
                .GetNotRemovedByPersistentLocalId(addressPersistentLocalId)
                .ChangePostalCode(postalCode);
        }

        public void CorrectAddressPosition(
            AddressPersistentLocalId addressPersistentLocalId,
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification,
            ExtendedWkbGeometry position)
        {
            GuardStreetNameStatusForChangeAndCorrection();

            StreetNameAddresses
                .GetNotRemovedByPersistentLocalId(addressPersistentLocalId)
                .CorrectPosition(geometryMethod, geometrySpecification, position);
        }

        public void CorrectAddressPostalCode(AddressPersistentLocalId addressPersistentLocalId, PostalCode postalCode, MunicipalityId municipalityIdByPostalCode)
        {
            GuardStreetNameStatusForChangeAndCorrection();

            StreetNameAddresses
                .GetNotRemovedByPersistentLocalId(addressPersistentLocalId)
                .CorrectPostalCode(postalCode, () => GuardPostalCodeMunicipalityMatchesStreetNameMunicipality(municipalityIdByPostalCode));
        }

        public void CorrectAddressHouseNumber(AddressPersistentLocalId addressPersistentLocalId, HouseNumber houseNumber)
        {
            GuardStreetNameStatusForChangeAndCorrection();

            StreetNameAddresses
                .GetNotRemovedByPersistentLocalId(addressPersistentLocalId)
                .CorrectHouseNumber(houseNumber, () => GuardAddressIsUnique(addressPersistentLocalId, houseNumber, null));
        }

        public void CorrectAddressBoxNumber(AddressPersistentLocalId addressPersistentLocalId, BoxNumber boxNumber)
        {
            GuardStreetNameStatusForChangeAndCorrection();

            var addressToCorrect = StreetNameAddresses.GetByPersistentLocalId(addressPersistentLocalId);
            addressToCorrect.CorrectBoxNumber(
                boxNumber,
                () => GuardAddressIsUnique(
                    addressPersistentLocalId,
                    addressToCorrect.HouseNumber,
                    boxNumber));
        }

        public void CorrectAddressApproval(AddressPersistentLocalId addressPersistentLocalId)
        {
            GuardStreetNameStatusForChangeAndCorrection();

            StreetNameAddresses
                .GetNotRemovedByPersistentLocalId(addressPersistentLocalId)
                .CorrectApproval();
        }

        public void CorrectAddressRetirement(AddressPersistentLocalId addressPersistentLocalId)
        {
            GuardStreetNameStatusForChangeAndCorrection();

            var addressToCorrect = StreetNameAddresses
                .GetNotRemovedByPersistentLocalId(addressPersistentLocalId);

            if (addressToCorrect.IsBoxNumberAddress)
            {
                var parent = addressToCorrect.Parent;

                if (parent == null || parent.IsRemoved)
                {
                    throw new ParentAddressNotFoundException(PersistentLocalId, addressToCorrect.HouseNumber);
                }

                if (parent.Status is AddressStatus.Proposed or AddressStatus.Rejected or AddressStatus.Retired)
                {
                    throw new ParentAddressHasInvalidStatusException();
                }
            }

            addressToCorrect.CorrectRetirement(() => GuardAddressIsUnique(
                addressToCorrect.AddressPersistentLocalId,
                addressToCorrect.HouseNumber,
                addressToCorrect.BoxNumber));
        }

        public void CorrectAddressRejection(AddressPersistentLocalId addressPersistentLocalId)
        {
            GuardStreetNameStatusForChangeAndCorrection();

            var addressToCorrect = StreetNameAddresses
                .GetNotRemovedByPersistentLocalId(addressPersistentLocalId);

            if (addressToCorrect.IsBoxNumberAddress)
            {
                var parent = addressToCorrect.Parent;

                if (parent == null || parent.IsRemoved)
                {
                    throw new ParentAddressNotFoundException(PersistentLocalId, addressToCorrect.HouseNumber);
                }

                if (parent.Status is AddressStatus.Rejected or AddressStatus.Retired)
                {
                    throw new ParentAddressHasInvalidStatusException();
                }
            }

            addressToCorrect.CorrectAddressRejection(() => GuardAddressIsUnique(
                addressToCorrect.AddressPersistentLocalId,
                addressToCorrect.HouseNumber,
                addressToCorrect.BoxNumber));
        }

        public void RemoveAddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            StreetNameAddresses
                .GetByPersistentLocalId(addressPersistentLocalId)
                .Remove();
        }

        private void GuardPostalCodeMunicipalityMatchesStreetNameMunicipality(MunicipalityId municipalityIdByPostalCode)
        {
            if (municipalityIdByPostalCode != MunicipalityId)
            {
                throw new PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException();
            }
        }
        private void GuardStreetNameStatusForChangeAndCorrection()
        {
            if (Status != StreetNameStatus.Proposed && Status != StreetNameStatus.Current)
            {
                throw new StreetNameHasInvalidStatusException();
            }
        }

        private void GuardAddressIsUnique(AddressPersistentLocalId addressPersistentLocalId, HouseNumber houseNumber, BoxNumber boxNumber)
        {
            if (StreetNameAddresses.HasActiveAddressForOtherThan(houseNumber, boxNumber, addressPersistentLocalId))
            {
                throw new AddressAlreadyExistsException(houseNumber, boxNumber);
            }
        }

        private void GuardActiveStreetName(StreetNamePersistentLocalId streetNamePersistentLocalId)
        {
            if (IsRemoved)
            {
                throw new StreetNameIsRemovedException(streetNamePersistentLocalId);
            }

            var validStatuses = new[] { StreetNameStatus.Proposed, StreetNameStatus.Current };

            if (!validStatuses.Contains(Status))
            {
                throw new StreetNameHasInvalidStatusException();
            }
        }

        public void CorrectAddressRegularization(AddressPersistentLocalId addressPersistentLocalId)
        {
            StreetNameAddresses
                .GetNotRemovedByPersistentLocalId(addressPersistentLocalId)
                .CorrectRegularizedAddress();
        }

        public void CorrectAddressDeregulation(AddressPersistentLocalId addressPersistentLocalId)
        {
            StreetNameAddresses
                .GetNotRemovedByPersistentLocalId(addressPersistentLocalId)
                .CorrectDeregularizedAddress();
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
                MunicipalityId,
                MigratedNisCode,
                Status,
                IsRemoved,
                StreetNameAddresses);
        }

        public ISnapshotStrategy Strategy { get; }
    }
}
