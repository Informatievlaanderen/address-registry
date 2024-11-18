namespace AddressRegistry.StreetName.Events
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [HideEvent]
    [EventTags(Tag.StreetName)]
    [EventName(EventName)]
    [EventDescription("De straatnaam met status afgekeurd werd gecorrigeerd naar status voorgesteld.")]
    public class StreetNameWasCorrectedFromRejectedToProposed : IStreetNameEvent
    {
        public const string EventName = "StreetNameWasCorrectedFromRejectedToProposed"; // BE CAREFUL CHANGING THIS!!

        public int StreetNamePersistentLocalId { get; }
        public ProvenanceData Provenance { get; private set; }

        public StreetNameWasCorrectedFromRejectedToProposed(StreetNamePersistentLocalId streetNamePersistentLocalId)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
        }

        [JsonConstructor]
        private StreetNameWasCorrectedFromRejectedToProposed(
            int streetNamePersistentLocalId,
            ProvenanceData provenance)
            : this(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(StreetNamePersistentLocalId.ToString(CultureInfo.InvariantCulture));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
