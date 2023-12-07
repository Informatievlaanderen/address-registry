namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Address;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using AddressGeometry = AddressRegistry.StreetName.AddressGeometry;
    using AddressId = AddressRegistry.StreetName.AddressId;
    using AddressStatus = AddressRegistry.StreetName.AddressStatus;
    using BoxNumber = AddressRegistry.StreetName.BoxNumber;
    using HouseNumber = AddressRegistry.StreetName.HouseNumber;
    using PostalCode = AddressRegistry.StreetName.PostalCode;

    public class MigrateAddressToStreetName : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("bd715c73-1a82-4a53-9549-f433a7fe34f0");

        public AddressId AddressId { get; }
        public StreetNamePersistentLocalId StreetNamePersistentLocalId { get; }
        public AddressStreetNameId StreetNameId { get; }
        public AddressPersistentLocalId AddressPersistentLocalId { get; }
        public AddressStatus Status { get; }
        public HouseNumber HouseNumber { get; }
        public BoxNumber? BoxNumber { get; }
        public AddressGeometry Geometry { get; }
        public bool? OfficiallyAssigned { get; }
        public PostalCode? PostalCode { get; }
        public bool IsCompleted { get; }
        public bool IsRemoved { get; }
        public AddressId? ParentAddressId { get; }
        public Provenance Provenance { get; }

        public MigrateAddressToStreetName(
            AddressRegistry.Address.AddressId addressId,
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            StreetNameId streetNameId,
            PersistentLocalId addressPersistentLocalId,
            AddressRegistry.Address.AddressStatus status,
            AddressRegistry.Address.HouseNumber houseNumber,
            AddressRegistry.Address.BoxNumber? boxNumber,
            AddressRegistry.Address.AddressGeometry geometry,
            bool? officiallyAssigned,
            AddressRegistry.Address.PostalCode? postalCode,
            bool isComplete,
            bool isRemoved,
            AddressRegistry.Address.AddressId? parentAddressId,
            Provenance provenance)
        {
            AddressId = new AddressId(addressId);
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            StreetNameId = new AddressStreetNameId(streetNameId);
            AddressPersistentLocalId = new AddressPersistentLocalId(addressPersistentLocalId);
            Status = AddressStatusHelpers.ToStreetNameAddressStatus(status);
            HouseNumber = new HouseNumber(houseNumber);
            BoxNumber = boxNumber is null ? null : new BoxNumber(boxNumber);
            Geometry = new AddressGeometry(geometry.GeometryMethod, geometry.GeometrySpecification, geometry.Geometry);
            OfficiallyAssigned = officiallyAssigned;
            PostalCode = postalCode is null ? null : new PostalCode(postalCode);
            IsCompleted = isComplete;
            IsRemoved = isRemoved;
            ParentAddressId = parentAddressId is null ? null : new AddressId(parentAddressId);
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
            yield return PostalCode ?? string.Empty;
            yield return Geometry;
            yield return ParentAddressId ?? string.Empty;
            yield return OfficiallyAssigned.HasValue ? OfficiallyAssigned.Value : string.Empty;
        }
    }
}
