namespace AddressRegistry.StreetName.DataStructures
{
    using Newtonsoft.Json;

    public class ReaddressedAddressData
    {
        public int SourceAddressPersistentLocalId { get; }
        public int DestinationAddressPersistentLocalId { get; }
        public bool IsDestinationNewlyProposed { get; }
        public AddressStatus SourceStatus { get; }
        public string DestinationHouseNumber { get; }
        public string? SourceBoxNumber { get; }
        public string SourcePostalCode { get; }
        public GeometryMethod SourceGeometryMethod { get; }
        public GeometrySpecification SourceGeometrySpecification { get; }
        public string SourceExtendedWkbGeometry { get; }
        public bool SourceIsOfficiallyAssigned { get; }

        public ReaddressedAddressData(
            AddressPersistentLocalId sourceAddressPersistentLocalId,
            AddressPersistentLocalId destinationAddressPersistentLocalId,
            bool isDestinationNewlyProposed,
            AddressStatus sourceStatus,
            HouseNumber destinationHouseNumber,
            BoxNumber? boxNumber,
            PostalCode sourcePostalCode,
            AddressGeometry geometry,
            bool sourceIsOfficiallyAssigned)
        {
            SourceAddressPersistentLocalId = sourceAddressPersistentLocalId;
            DestinationAddressPersistentLocalId = destinationAddressPersistentLocalId;
            IsDestinationNewlyProposed = isDestinationNewlyProposed;
            SourceStatus = sourceStatus;
            DestinationHouseNumber = destinationHouseNumber;
            SourceBoxNumber = boxNumber is null ? (string?)null : boxNumber;
            SourcePostalCode = sourcePostalCode;
            SourceGeometryMethod = geometry.GeometryMethod;
            SourceGeometrySpecification = geometry.GeometrySpecification;
            SourceExtendedWkbGeometry = geometry.Geometry.ToString();
            SourceIsOfficiallyAssigned = sourceIsOfficiallyAssigned;
        }

        [JsonConstructor]
        private ReaddressedAddressData(
            int sourceAddressPersistentLocalId,
            int destinationAddressPersistentLocalId,
            bool isDestinationNewlyProposed,
            AddressStatus sourceStatus,
            string destinationHouseNumber,
            string? sourceBoxNumber,
            string sourcePostalCode,
            GeometryMethod sourceGeometryMethod,
            GeometrySpecification sourceGeometrySpecification,
            string sourceExtendedWkbGeometry,
            bool sourceIsOfficiallyAssigned,
            int? destinationParentAddressPersistentLocalId)
        {
            SourceAddressPersistentLocalId = sourceAddressPersistentLocalId;
            DestinationAddressPersistentLocalId = destinationAddressPersistentLocalId;
            IsDestinationNewlyProposed = isDestinationNewlyProposed;
            SourceStatus = sourceStatus;
            DestinationHouseNumber = destinationHouseNumber;
            SourceBoxNumber = sourceBoxNumber;
            SourcePostalCode = sourcePostalCode;
            SourceGeometryMethod = sourceGeometryMethod;
            SourceGeometrySpecification = sourceGeometrySpecification;
            SourceExtendedWkbGeometry = sourceExtendedWkbGeometry;
            SourceIsOfficiallyAssigned = sourceIsOfficiallyAssigned;
        }
    }
}
