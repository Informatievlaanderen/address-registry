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
    [EventDescription("De homoniemtoevoegingen van de straatnaam (in een bepaalde taal) werd(en) gecorrigeerd.")]
    public class StreetNameHomonymAdditionsWereCorrected : IStreetNameEvent
    {
        public const string EventName = "StreetNameHomonymAdditionsWereCorrected"; // BE CAREFUL CHANGING THIS!!

        public int StreetNamePersistentLocalId { get; }
        public IDictionary<string, string> StreetNameHomonymAdditions { get; }
        public IEnumerable<int> AddressPersistentLocalIds { get; }
        public ProvenanceData Provenance { get; private set; }

        public StreetNameHomonymAdditionsWereCorrected(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            IDictionary<string, string> streetNameHomonymAdditions,
            IEnumerable<AddressPersistentLocalId> addressPersistentLocalIds)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            StreetNameHomonymAdditions = streetNameHomonymAdditions;
            AddressPersistentLocalIds = addressPersistentLocalIds.Select(addressPersistentLocalId => (int)addressPersistentLocalId);
        }

        [JsonConstructor]
        private StreetNameHomonymAdditionsWereCorrected(
            int streetNamePersistentLocalId,
            IDictionary<string, string> streetNameHomonymAdditions,
            IEnumerable<int> addressPersistentLocalIds,
            ProvenanceData provenance)
            : this(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                streetNameHomonymAdditions,
                addressPersistentLocalIds.Select(addressPersistentLocalId => new AddressPersistentLocalId(addressPersistentLocalId)).ToList())
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(StreetNamePersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.AddRange(AddressPersistentLocalIds.Select(x => x.ToString(CultureInfo.InvariantCulture)));
            fields.AddRange(StreetNameHomonymAdditions.Select(streetNameName => $"{streetNameName.Key}: {streetNameName.Value}"));

            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
