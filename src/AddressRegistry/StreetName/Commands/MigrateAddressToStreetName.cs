namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Address;
    using AddressRegistry.StreetName;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class MigrateAddressToStreetName : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("bd715c73-1a82-4a53-9549-f433a7fe34f0");

        public AddressRegistry.StreetName.AddressId AddressId { get; }
        public StreetNamePersistentLocalId StreetNamePersistentLocalId { get; }
        public AddressStreetNameId StreetNameId { get; }
        public AddressPersistentLocalId AddressPersistentLocalId { get; }
        public AddressRegistry.StreetName.AddressStatus Status { get; }
        public AddressRegistry.StreetName.HouseNumber HouseNumber { get; }
        public AddressRegistry.StreetName.BoxNumber? BoxNumber { get; }
        public AddressRegistry.StreetName.AddressGeometry Geometry { get; }
        public bool? OfficiallyAssigned { get; }
        public AddressRegistry.StreetName.PostalCode PostalCode { get; }
        public bool IsCompleted { get; }
        public bool IsRemoved { get; }
        public AddressRegistry.StreetName.AddressId? ParentAddressId { get; }
        public Provenance Provenance { get; }

        public MigrateAddressToStreetName(
            AddressRegistry.Address.AddressId addressId,
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressRegistry.Address.StreetNameId streetNameId,
            PersistentLocalId addressPersistentLocalId,
            AddressRegistry.Address.AddressStatus status,
            AddressRegistry.Address.HouseNumber houseNumber,
            AddressRegistry.Address.BoxNumber? boxNumber,
            AddressRegistry.Address.AddressGeometry geometry,
            bool? officiallyAssigned,
            AddressRegistry.Address.PostalCode postalCode,
            bool isComplete,
            bool isRemoved,
            AddressRegistry.Address.AddressId? parentAddressId,
            Provenance provenance)
        {
            AddressId = new AddressRegistry.StreetName.AddressId(addressId);
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            StreetNameId = new AddressStreetNameId(streetNameId);
            AddressPersistentLocalId = new AddressPersistentLocalId(addressPersistentLocalId);
            Status = status.ToStreetNameAddressStatus();
            HouseNumber = new AddressRegistry.StreetName.HouseNumber(houseNumber);
            BoxNumber = boxNumber == null ? (AddressRegistry.StreetName.BoxNumber?)null : new AddressRegistry.StreetName.BoxNumber(boxNumber);
            Geometry = new AddressRegistry.StreetName.AddressGeometry(geometry.GeometryMethod, geometry.GeometrySpecification, geometry.Geometry);
            OfficiallyAssigned = officiallyAssigned;
            PostalCode = new AddressRegistry.StreetName.PostalCode(postalCode);
            IsCompleted = isComplete;
            IsRemoved = isRemoved;
            ParentAddressId = parentAddressId == null ? (AddressRegistry.StreetName.AddressId?)null : new AddressRegistry.StreetName.AddressId(parentAddressId);
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"MigrateAddressToStreetName-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return StreetNameId;
            yield return IsCompleted;
            yield return Status;
            yield return AddressPersistentLocalId;
            yield return AddressId;
            yield return StreetNamePersistentLocalId;
            yield return IsRemoved;
            yield return BoxNumber ?? string.Empty;
            yield return HouseNumber;
            yield return PostalCode;
            yield return Geometry;
            yield return ParentAddressId ?? string.Empty;
            yield return OfficiallyAssigned.HasValue ? OfficiallyAssigned.Value : string.Empty;
        }
    }
}
