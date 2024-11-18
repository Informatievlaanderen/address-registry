namespace AddressRegistry.StreetName.Events
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [HideEvent]
    [EventTags(Tag.StreetName)]
    [EventName(EventName)]
    [EventDescription("De straatnaam werd afgekeurd in functie van een gemeentefusie.")]
    public class StreetNameWasRejectedBecauseOfMunicipalityMerger : IStreetNameEvent
    {
        public const string EventName = "StreetNameWasRejectedBecauseOfMunicipalityMerger"; // BE CAREFUL CHANGING THIS!!

        public int StreetNamePersistentLocalId { get; }
        public List<int> NewStreetNamePersistentLocalIds { get; }
        public ProvenanceData Provenance { get; private set; }

        public StreetNameWasRejectedBecauseOfMunicipalityMerger(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            IEnumerable<StreetNamePersistentLocalId> newStreetNamePersistentLocalIds)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            NewStreetNamePersistentLocalIds = newStreetNamePersistentLocalIds.Select(x => (int)x).ToList();
        }

        [JsonConstructor]
        private StreetNameWasRejectedBecauseOfMunicipalityMerger(
            int streetNamePersistentLocalId,
            IEnumerable<int> newStreetNamePersistentLocalIds,
            ProvenanceData provenance)
            : this(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                newStreetNamePersistentLocalIds.Select(x => new StreetNamePersistentLocalId(x)))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(StreetNamePersistentLocalId.ToString());
            fields.AddRange(NewStreetNamePersistentLocalIds.Select(x => x.ToString()));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
