namespace AddressRegistry.StreetName.DataStructures
{
    using Newtonsoft.Json;

    public class ReaddressAddressData
    {
        public int SourceAddressPersistentLocalId { get; }
        public int DestinationAddressPersistentLocalId { get; }
        public AddressStatus SourceStatus { get; }

        public string SourceHouseNumber { get; }

        //public string? SourceBoxNumber { get; }
        public string SourcePostalCode { get; }

        public GeometryMethod SourceGeometryMethod { get; }
        public GeometrySpecification SourceGeometrySpecification { get; }
        public string SourceExtendedWkbGeometry { get; }

        public bool SourceIsOfficiallyAssigned { get; }
        //public int? SourceParentId { get; }

        public ReaddressAddressData(
            AddressPersistentLocalId sourceAddressPersistentLocalId,
            AddressPersistentLocalId destinationAddressPersistentLocalId,
            AddressStatus sourceStatus,
            HouseNumber sourceHouseNumber,
            //BoxNumber? boxNumber,
            PostalCode sourcePostalCode,
            AddressGeometry geometry,
            bool sourceIsOfficiallyAssigned
            //StreetNameAddress? parent
        )
        {
            SourceAddressPersistentLocalId = sourceAddressPersistentLocalId;
            DestinationAddressPersistentLocalId = destinationAddressPersistentLocalId;
            SourceStatus = sourceStatus;
            SourceHouseNumber = sourceHouseNumber;
            //BoxNumber = boxNumber is null ? (string?)null : boxNumber;
            SourcePostalCode = sourcePostalCode;
            SourceGeometryMethod = geometry.GeometryMethod;
            SourceGeometrySpecification = geometry.GeometrySpecification;
            SourceExtendedWkbGeometry = geometry.Geometry.ToString();
            SourceIsOfficiallyAssigned = sourceIsOfficiallyAssigned;
            // ParentId = parent is null ? (int?)null : parent.AddressPersistentLocalId;
        }

        [JsonConstructor]
        private ReaddressAddressData(
            int sourceAddressPersistentLocalId,
            int destinationAddressPersistentLocalId,
            AddressStatus sourceStatus,
            string sourceHouseNumber,
            //string? boxNumber,
            string sourcePostalCode,
            GeometryMethod sourceGeometryMethod,
            GeometrySpecification sourceGeometrySpecification,
            string sourceExtendedWkbGeometry,
            bool sourceIsOfficiallyAssigned
            //int? parentId
            )
        {
            SourceAddressPersistentLocalId = sourceAddressPersistentLocalId;
            DestinationAddressPersistentLocalId = destinationAddressPersistentLocalId;
            SourceStatus = sourceStatus;
            SourceHouseNumber = sourceHouseNumber;
            //BoxNumber = boxNumber;
            SourcePostalCode = sourcePostalCode;
            SourceGeometryMethod = sourceGeometryMethod;
            SourceGeometrySpecification = sourceGeometrySpecification;
            SourceExtendedWkbGeometry = sourceExtendedWkbGeometry;
            SourceIsOfficiallyAssigned = sourceIsOfficiallyAssigned;
            //ParentId = parentId;
        }
    }
}
