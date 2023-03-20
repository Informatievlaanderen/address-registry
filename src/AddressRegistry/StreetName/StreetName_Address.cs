namespace AddressRegistry.StreetName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Events;
    using Exceptions;

    public partial class StreetName
    {
        public void MigrateAddress(
            AddressId addressId,
            AddressStreetNameId streetNameId,
            AddressPersistentLocalId addressPersistentLocalId,
            AddressStatus addressStatus,
            HouseNumber houseNumber,
            BoxNumber boxNumber,
            AddressGeometry geometry,
            bool? officiallyAssigned,
            PostalCode? postalCode,
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
            AddressPersistentLocalId addressPersistentLocalId,
            PostalCode postalCode,
            MunicipalityId municipalityIdByPostalCode,
            HouseNumber houseNumber,
            BoxNumber? boxNumber,
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification,
            ExtendedWkbGeometry geometryPosition)
        {
            GuardActiveStreetName(PersistentLocalId);

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
                throw new ParentAddressNotFoundException(PersistentLocalId, houseNumber);
            }

            if (isChild && !parent.BoxNumberIsUnique(boxNumber!))
            {
                throw new AddressAlreadyExistsException(houseNumber, boxNumber!);
            }

            if (isChild && parent.PostalCode != postalCode)
            {
                throw new BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException();
            }

            StreetNameAddress.GuardGeometry(geometryMethod, geometrySpecification);

            ApplyChange(new AddressWasProposedV2(
                PersistentLocalId,
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

        public void RetireAddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            StreetNameAddresses
                .GetNotRemovedByPersistentLocalId(addressPersistentLocalId)
                .Retire();
        }

        public void RemoveAddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            StreetNameAddresses
                .GetByPersistentLocalId(addressPersistentLocalId)
                .Remove();
        }

        public void RegularizeAddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            StreetNameAddresses
                .GetNotRemovedByPersistentLocalId(addressPersistentLocalId)
                .Regularize();
        }

        public void DeregulateAddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            StreetNameAddresses
                .GetNotRemovedByPersistentLocalId(addressPersistentLocalId)
                .Deregulate();
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

            addressToCorrect.CorrectRejection(() => GuardAddressIsUnique(
                addressToCorrect.AddressPersistentLocalId,
                addressToCorrect.HouseNumber,
                addressToCorrect.BoxNumber));
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

        public void CorrectAddressRegularization(AddressPersistentLocalId addressPersistentLocalId)
        {
            StreetNameAddresses
                .GetNotRemovedByPersistentLocalId(addressPersistentLocalId)
                .CorrectRegularization();
        }

        public void CorrectAddressDeregulation(AddressPersistentLocalId addressPersistentLocalId)
        {
            StreetNameAddresses
                .GetNotRemovedByPersistentLocalId(addressPersistentLocalId)
                .CorrectDeregulation();
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

        public string GetAddressHash(AddressPersistentLocalId addressPersistentLocalId)
        {
            var address = StreetNameAddresses.FindByPersistentLocalId(addressPersistentLocalId);
            if (address == null)
            {
                throw new AggregateSourceException($"Cannot find a address entity with id {addressPersistentLocalId}");
            }

            return address.LastEventHash;
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

        private void GuardAddressIsUnique(AddressPersistentLocalId addressPersistentLocalId, HouseNumber houseNumber, BoxNumber boxNumber)
        {
            if (StreetNameAddresses.HasActiveAddressForOtherThan(houseNumber, boxNumber, addressPersistentLocalId))
            {
                throw new AddressAlreadyExistsException(houseNumber, boxNumber);
            }
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
    }
}
