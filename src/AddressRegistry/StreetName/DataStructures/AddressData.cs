namespace AddressRegistry.StreetName.DataStructures
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    public class AddressData
    {
        public int AddressPersistentLocalId { get; }
        public AddressStatus Status { get; }
        public string HouseNumber { get; }
        public string? BoxNumber { get; }
        public string? PostalCode { get; }

        public GeometryMethod? GeometryMethod { get; }
        public GeometrySpecification? GeometrySpecification { get; }
        public string? ExtendedWkbGeometry { get; }

        public bool IsOfficiallyAssigned { get; }
        public bool IsRemoved { get; }
        public int? ParentId { get; }
        public int? MergedAddressPersistentLocalId { get; }

        public Guid? LegacyAddressId { get; }
        public string LastEventHash { get; }
        public ProvenanceData LastProvenanceData { get; }

        public AddressData(StreetNameAddress address)
            : this(address.AddressPersistentLocalId,
                address.Status,
                address.HouseNumber,
                address.BoxNumber,
                address.PostalCode,
                address.Geometry,
                address.IsOfficiallyAssigned,
                address.IsRemoved,
                address.Parent,
                address.MergedAddressPersistentLocalId,
                address.LegacyAddressId,
                address.LastEventHash,
                address.LastProvenanceData)
        { }

        public AddressData(
            AddressPersistentLocalId addressPersistentLocalId,
            AddressStatus addressStatus,
            HouseNumber houseNumber,
            BoxNumber? boxNumber,
            PostalCode? postalCode,
            AddressGeometry? geometry,
            bool isOfficiallyAssigned,
            bool isRemoved,
            StreetNameAddress? parent,
            AddressPersistentLocalId? mergedAddressPersistentLocalId,
            AddressId? legacyAddressId,
            string lastEventHash,
            ProvenanceData lastProvenanceData)
        {
            AddressPersistentLocalId = addressPersistentLocalId;
            Status = addressStatus;

            HouseNumber = houseNumber;
            BoxNumber = boxNumber is null ? (string?)null : boxNumber;
            PostalCode = postalCode is null ? (string?)null : postalCode;

            if (geometry is not null)
            {
                GeometryMethod = geometry.GeometryMethod;
                GeometrySpecification = geometry.GeometrySpecification;
                ExtendedWkbGeometry = geometry.Geometry.ToString();
            }

            IsOfficiallyAssigned = isOfficiallyAssigned;
            IsRemoved = isRemoved;
            ParentId = parent is null ? (int?)null : parent.AddressPersistentLocalId;
            MergedAddressPersistentLocalId = mergedAddressPersistentLocalId is null ? (int?)null : mergedAddressPersistentLocalId;
            LegacyAddressId = legacyAddressId is null ? (Guid?)null : legacyAddressId;
            LastEventHash = lastEventHash;
            LastProvenanceData = lastProvenanceData;
        }

        [JsonConstructor]
        private AddressData(
            int addressPersistentLocalId,
            AddressStatus status,
            string houseNumber,
            string? boxNumber,
            string postalCode,
            GeometryMethod? geometryMethod,
            GeometrySpecification? geometrySpecification,
            string? extendedWkbGeometry,
            bool isOfficiallyAssigned,
            bool isRemoved,
            int? parentId,
            int? mergedAddressPersistentLocalId,
            Guid? legacyAddressId,
            string lastEventHash,
            ProvenanceData lastProvenanceData)
        {
            AddressPersistentLocalId = addressPersistentLocalId;
            Status = status;
            HouseNumber = houseNumber;
            BoxNumber = boxNumber;
            PostalCode = postalCode;

            GeometryMethod = geometryMethod;
            GeometrySpecification = geometrySpecification;
            ExtendedWkbGeometry = extendedWkbGeometry;

            IsOfficiallyAssigned = isOfficiallyAssigned;
            IsRemoved = isRemoved;

            ParentId = parentId;
            MergedAddressPersistentLocalId = mergedAddressPersistentLocalId;
            LegacyAddressId = legacyAddressId;
            LastEventHash = lastEventHash;
            LastProvenanceData = lastProvenanceData;
        }
    }
}
