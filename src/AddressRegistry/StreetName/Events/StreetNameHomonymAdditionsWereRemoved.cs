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
    [EventDescription("De homoniemtoevoegingen van de straatnaam (in een bepaalde taal) werd(en) verwijderd.")]
    public class StreetNameHomonymAdditionsWereRemoved : IStreetNameEvent
    {
        public const string EventName = "StreetNameHomonymAdditionsWereRemoved"; // BE CAREFUL CHANGING THIS!!

        public int StreetNamePersistentLocalId { get; }
        public IList<string> StreetNameHomonymAdditionLanguages { get; }
        public IEnumerable<int> AddressPersistentLocalIds { get; }
        public ProvenanceData Provenance { get; private set; }

        public StreetNameHomonymAdditionsWereRemoved(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            IEnumerable<string> streetNameHomonymAdditionLanguages,
            IEnumerable<AddressPersistentLocalId> addressPersistentLocalIds)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            StreetNameHomonymAdditionLanguages = streetNameHomonymAdditionLanguages.ToList();
            AddressPersistentLocalIds = addressPersistentLocalIds.Select(addressPersistentLocalId => (int)addressPersistentLocalId);
        }

        [JsonConstructor]
        private StreetNameHomonymAdditionsWereRemoved(
            int streetNamePersistentLocalId,
            IList<string> streetNameHomonymAdditionLanguages,
            IEnumerable<int> addressPersistentLocalIds,
            ProvenanceData provenance)
            : this(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                streetNameHomonymAdditionLanguages,
                addressPersistentLocalIds.Select(addressPersistentLocalId => new AddressPersistentLocalId(addressPersistentLocalId)).ToList())
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(StreetNamePersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.AddRange(AddressPersistentLocalIds.Select(x => x.ToString(CultureInfo.InvariantCulture)));
            fields.AddRange(StreetNameHomonymAdditionLanguages);

            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
