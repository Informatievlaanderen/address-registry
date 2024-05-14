namespace AddressRegistry.StreetName.DataStructures
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.EventHandling;

    public class AddressHouseNumberReaddressedData
    {
        [EventPropertyDescription("Objectidentificator van het doelHuisnummer.")]
        public int AddressPersistentLocalId { get; }

        [EventPropertyDescription("Een lijst van de eigenschappen die het doelHuisnummer van het bronHuisnummer overneemt.")]
        public ReaddressedAddressData ReaddressedHouseNumber { get; }

        [EventPropertyDescription(
            "Een lijst van de gekoppelde busnummers aan het huisnummer met per busnummer een lijst van de eigenschappen die het doelBusnummer van het bronBusnummer overneemt.")]
        public IReadOnlyList<ReaddressedAddressData> ReaddressedBoxNumbers { get; }

        public AddressHouseNumberReaddressedData(
            int addressPersistentLocalId,
            ReaddressedAddressData readdressedHouseNumber,
            IReadOnlyList<ReaddressedAddressData> readdressedBoxNumbers)
        {
            AddressPersistentLocalId = addressPersistentLocalId;
            ReaddressedHouseNumber = readdressedHouseNumber;
            ReaddressedBoxNumbers = readdressedBoxNumbers;
        }

        public IEnumerable<string> GetHashFields()
        {
            var fields = new List<string>
            {
                ReaddressedHouseNumber.SourceAddressPersistentLocalId.ToString(),
                ReaddressedHouseNumber.DestinationAddressPersistentLocalId.ToString(),
                ReaddressedHouseNumber.IsDestinationNewlyProposed.ToString(),
                ReaddressedHouseNumber.DestinationHouseNumber,
                ReaddressedHouseNumber.SourcePostalCode,
                ReaddressedHouseNumber.SourceStatus.ToString(),
                ReaddressedHouseNumber.SourceGeometryMethod.ToString(),
                ReaddressedHouseNumber.SourceGeometrySpecification.ToString(),
                ReaddressedHouseNumber.SourceExtendedWkbGeometry,
                ReaddressedHouseNumber.SourceIsOfficiallyAssigned.ToString()
            };

            foreach (var item in ReaddressedBoxNumbers)
            {
                fields.Add(item.SourceAddressPersistentLocalId.ToString());
                fields.Add(item.DestinationAddressPersistentLocalId.ToString());
                fields.Add(item.IsDestinationNewlyProposed.ToString());
                fields.Add(item.DestinationHouseNumber);
                fields.Add(item.SourcePostalCode);
                fields.Add(item.SourceStatus.ToString());
                fields.Add(item.SourceGeometryMethod.ToString());
                fields.Add(item.SourceGeometrySpecification.ToString());
                fields.Add(item.SourceExtendedWkbGeometry);
                fields.Add(item.SourceIsOfficiallyAssigned.ToString());
            }

            return fields;
        }
    }
}
