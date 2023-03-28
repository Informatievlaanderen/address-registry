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
    [EventDescription("Het adres werd geheradresseerd.")]
    public class StreetNameWasReaddressed : IStreetNameEvent
    {
        public const string EventName = "StreetNameWasReaddressed"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van de straatnaam aan dewelke de adressen zijn toegewezen.")]
        public int StreetNamePersistentLocalId { get; }

        [EventPropertyDescription("De voorgestelde adressen.")]
        public IReadOnlyList<int> ProposedAddressPersistentLocalIds { get; }

        [EventPropertyDescription("De heradresseerde adressen.")]
        public IReadOnlyList<ReaddressAddressData> ReaddressedAddresses { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNameWasReaddressed(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            List<AddressPersistentLocalId> proposedAddressPersistentLocalIds,
            List<ReaddressAddressData> readdressedAddresses)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            ProposedAddressPersistentLocalIds = proposedAddressPersistentLocalIds.Select(x => (int)x).ToList();
            ReaddressedAddresses = readdressedAddresses;
        }

        [JsonConstructor]
        private StreetNameWasReaddressed(
            int streetNamePersistentLocalId,
            List<int> proposedAddressPersistentLocalIds,
            List<ReaddressAddressData> readdressedAddresses,
            ProvenanceData provenance)
            : this(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                proposedAddressPersistentLocalIds.Select(x => new AddressPersistentLocalId(x)).ToList(),
                readdressedAddresses)
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(StreetNamePersistentLocalId.ToString(CultureInfo.InvariantCulture));

            foreach (var item in ProposedAddressPersistentLocalIds)
            {
                fields.Add(item.ToString());
            }

            foreach (var item in ReaddressedAddresses)
            {
                fields.Add(item.SourceAddressPersistentLocalId.ToString());
                fields.Add(item.DestinationAddressPersistentLocalId.ToString());
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
