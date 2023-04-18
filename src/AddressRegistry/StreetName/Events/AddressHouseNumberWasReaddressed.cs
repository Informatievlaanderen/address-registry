namespace AddressRegistry.StreetName.Events
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using DataStructures;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Edit, EventTag.For.Sync)]
    [EventName(EventName)]
    [EventDescription("Het huisnummeradres werd geheradresseerd.")]
    public class AddressHouseNumberWasReaddressed : IStreetNameEvent, IHasAddressPersistentLocalId
    {
        public const string EventName = "AddressHouseNumberWasReaddressed"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van de straatnaam aan dewelke de adressen zijn toegewezen.")]
        public int StreetNamePersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van het adres.")]
        public int AddressPersistentLocalId { get; }

        [EventPropertyDescription("Het heradresseerde huisnummeradres.")]
        public ReaddressedAddressData ReaddressedHouseNumber { get; }

        [EventPropertyDescription("De heradresseerde busadressen.")]
        public IReadOnlyList<ReaddressedAddressData> ReaddressedBoxNumbers { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public AddressHouseNumberWasReaddressed(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            ReaddressedAddressData readdressedHouseNumber,
            List<ReaddressedAddressData> readdressedBoxNumbers)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            AddressPersistentLocalId = addressPersistentLocalId;
            ReaddressedHouseNumber = readdressedHouseNumber;
            ReaddressedBoxNumbers = readdressedBoxNumbers;
        }

        [JsonConstructor]
        private AddressHouseNumberWasReaddressed(
            int streetNamePersistentLocalId,
            int addressPersistentLocalId,
            ReaddressedAddressData readdressedHouseNumber,
            List<ReaddressedAddressData> readdressedBoxNumbers,
            List<int> rejectedBoxNumberAddressPersistentLocalIds,
            List<int> retiredBoxNumberAddressPersistentLocalIds,
            ProvenanceData provenance)
            : this(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                new AddressPersistentLocalId(addressPersistentLocalId),
                readdressedHouseNumber,
                readdressedBoxNumbers)
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(StreetNamePersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(AddressPersistentLocalId.ToString(CultureInfo.InvariantCulture));

            fields.Add(ReaddressedHouseNumber.SourceAddressPersistentLocalId.ToString());
            fields.Add(ReaddressedHouseNumber.DestinationAddressPersistentLocalId.ToString());
            fields.Add(ReaddressedHouseNumber.IsDestinationNewlyProposed.ToString());
            fields.Add(ReaddressedHouseNumber.DestinationHouseNumber);
            fields.Add(ReaddressedHouseNumber.SourcePostalCode);
            fields.Add(ReaddressedHouseNumber.SourceStatus.ToString());
            fields.Add(ReaddressedHouseNumber.SourceGeometryMethod.ToString());
            fields.Add(ReaddressedHouseNumber.SourceGeometrySpecification.ToString());
            fields.Add(ReaddressedHouseNumber.SourceExtendedWkbGeometry);
            fields.Add(ReaddressedHouseNumber.SourceIsOfficiallyAssigned.ToString());

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

        public string GetHash() => this.ToEventHash(EventName);
    }
}
