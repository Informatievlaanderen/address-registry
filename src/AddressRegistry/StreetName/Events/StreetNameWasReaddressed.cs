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

    [EventName(EventName)]
    [EventDescription("Er werden huisnummers geheradresseerd in de straatnaam.")]
    [EventTags(Tag.StreetName)]
    public class StreetNameWasReaddressed : IStreetNameEvent
    {
        public const string EventName = "StreetNameWasReaddressed"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van de straatnaam aan dewelke de adressen zijn toegewezen.")]
        public int StreetNamePersistentLocalId { get; }

        [EventPropertyDescription("Een lijst van de huisnummers die werden geheradresseerd.")]
        public IReadOnlyList<AddressHouseNumberReaddressedData> ReaddressedHouseNumbers { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNameWasReaddressed(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            IEnumerable<AddressHouseNumberWasReaddressed> addressHouseNumberWasReaddressedEvents)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            ReaddressedHouseNumbers = addressHouseNumberWasReaddressedEvents.Select(x
                => new AddressHouseNumberReaddressedData
                (
                    x.AddressPersistentLocalId,
                    x.ReaddressedHouseNumber,
                    x.ReaddressedBoxNumbers.ToList().AsReadOnly()
                )).ToList().AsReadOnly();
        }

        [JsonConstructor]
        private StreetNameWasReaddressed(
            int streetNamePersistentLocalId,
            List<AddressHouseNumberReaddressedData> readdressedHouseNumbers,
            ProvenanceData provenance)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            ReaddressedHouseNumbers = readdressedHouseNumbers.AsReadOnly();
            ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());
        }

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(StreetNamePersistentLocalId.ToString(CultureInfo.InvariantCulture));

            foreach (var addressHouseNumberReaddressedData in ReaddressedHouseNumbers)
                fields.AddRange(addressHouseNumberReaddressedData.GetHashFields());

            return fields;
        }

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public string GetHash() => this.ToEventHash(EventName);
    }
}
