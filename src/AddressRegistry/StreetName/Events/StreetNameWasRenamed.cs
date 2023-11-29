namespace AddressRegistry.StreetName.Events
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(Tag.StreetName)]
    [EventName(EventName)]
    [EventDescription("De straatnaam werd hernoemd.")]
    public class StreetNameWasRenamed : IStreetNameEvent
    {
        public const string EventName = "StreetNameWasRenamed"; // BE CAREFUL CHANGING THIS!!

        public int StreetNamePersistentLocalId { get; }
        public int DestinationStreetNamePersistentLocalId { get; }
        public ProvenanceData Provenance { get; private set; }

        public StreetNameWasRenamed(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            StreetNamePersistentLocalId destinationStreetNamePersistentLocalId)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            DestinationStreetNamePersistentLocalId = destinationStreetNamePersistentLocalId;
        }

        [JsonConstructor]
        private StreetNameWasRenamed(
            int streetNamePersistentLocalId,
            int destinationStreetNamePersistentLocalId,
            ProvenanceData provenance)
            : this(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                new StreetNamePersistentLocalId(destinationStreetNamePersistentLocalId))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(StreetNamePersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(DestinationStreetNamePersistentLocalId.ToString(CultureInfo.InvariantCulture));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
