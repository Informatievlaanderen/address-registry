namespace AddressRegistry.StreetName.Events
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Edit, EventTag.For.Sync)]
    [EventName(EventName)]
    [EventDescription("Het adres werd afgekeurd in functie van een gemeentefusie.")]
    public class AddressWasRejectedBecauseOfMunicipalityMerger : IStreetNameEvent, IHasAddressPersistentLocalId
    {
        public const string EventName = "AddressWasRejectedBecauseOfMunicipalityMerger"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van de straatnaam aan dewelke het adres is toegewezen.")]
        public int StreetNamePersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van het adres.")]
        public int AddressPersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van het nieuwe adres (optioneel).")]
        public int? NewAddressPersistentLocalId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public AddressWasRejectedBecauseOfMunicipalityMerger(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            AddressPersistentLocalId? newAddressPersistentLocalId)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            AddressPersistentLocalId = addressPersistentLocalId;
            NewAddressPersistentLocalId = newAddressPersistentLocalId is not null ? (int?)newAddressPersistentLocalId : null;
        }

        [JsonConstructor]
        private AddressWasRejectedBecauseOfMunicipalityMerger(
            int streetNamePersistentLocalId,
            int addressPersistentLocalId,
            int? newAddressPersistentLocalId,
            ProvenanceData provenance)
            : this(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                new AddressPersistentLocalId(addressPersistentLocalId),
                newAddressPersistentLocalId is not null ? new AddressPersistentLocalId(newAddressPersistentLocalId.Value) : null)
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(StreetNamePersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(AddressPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            if (NewAddressPersistentLocalId.HasValue)
            {
                fields.Add(NewAddressPersistentLocalId.Value.ToString(CultureInfo.InvariantCulture));
            }
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
