namespace AddressRegistry.StreetName.DataStructures
{
    using Newtonsoft.Json;

    public class ReaddressAddressData
    {
        public int SourceAddressPersistentLocalId { get; }
        public int DestinationAddressPersistentLocalId { get; }
        public AddressStatus SourceStatus { get; }
        public string DestinationHouseNumber { get; }
        public string? SourceBoxNumber { get; }
        public string SourcePostalCode { get; }
        public GeometryMethod SourceGeometryMethod { get; }
        public GeometrySpecification SourceGeometrySpecification { get; }
        public string SourceExtendedWkbGeometry { get; }
        public bool SourceIsOfficiallyAssigned { get; }
        public int? DestinationParentId { get; }
        public bool IsBoxNumberAddress => SourceBoxNumber is not null;
        public bool IsHouseNumberAddress => !IsBoxNumberAddress;

        public ReaddressAddressData(
            AddressPersistentLocalId sourceAddressPersistentLocalId,
            AddressPersistentLocalId destinationAddressPersistentLocalId,
            AddressStatus sourceStatus,
            HouseNumber destinationHouseNumber,
            BoxNumber? boxNumber,
            PostalCode sourcePostalCode,
            AddressGeometry geometry,
            bool sourceIsOfficiallyAssigned,
            AddressPersistentLocalId? parentAddressPersistentLocalId
        )
        {
            SourceAddressPersistentLocalId = sourceAddressPersistentLocalId;
            DestinationAddressPersistentLocalId = destinationAddressPersistentLocalId;
            SourceStatus = sourceStatus;
            DestinationHouseNumber = destinationHouseNumber;
            SourceBoxNumber = boxNumber is null ? (string?)null : boxNumber;
            SourcePostalCode = sourcePostalCode;
            SourceGeometryMethod = geometry.GeometryMethod;
            SourceGeometrySpecification = geometry.GeometrySpecification;
            SourceExtendedWkbGeometry = geometry.Geometry.ToString();
            SourceIsOfficiallyAssigned = sourceIsOfficiallyAssigned;
            DestinationParentId = parentAddressPersistentLocalId ?? (int?) null;
        }

        [JsonConstructor]
        private ReaddressAddressData(
            int sourceAddressPersistentLocalId,
            int destinationAddressPersistentLocalId,
            AddressStatus sourceStatus,
            string destinationHouseNumber,
            string? sourceBoxNumber,
            string sourcePostalCode,
            GeometryMethod sourceGeometryMethod,
            GeometrySpecification sourceGeometrySpecification,
            string sourceExtendedWkbGeometry,
            bool sourceIsOfficiallyAssigned,
            int? destinationParentId)
        {
            SourceAddressPersistentLocalId = sourceAddressPersistentLocalId;
            DestinationAddressPersistentLocalId = destinationAddressPersistentLocalId;
            SourceStatus = sourceStatus;
            DestinationHouseNumber = destinationHouseNumber;
            SourceBoxNumber = sourceBoxNumber;
            SourcePostalCode = sourcePostalCode;
            SourceGeometryMethod = sourceGeometryMethod;
            SourceGeometrySpecification = sourceGeometrySpecification;
            SourceExtendedWkbGeometry = sourceExtendedWkbGeometry;
            SourceIsOfficiallyAssigned = sourceIsOfficiallyAssigned;
            DestinationParentId = destinationParentId;
        }
    }
}
