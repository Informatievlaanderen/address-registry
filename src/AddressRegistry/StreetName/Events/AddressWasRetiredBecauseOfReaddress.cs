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
    [EventDescription("Het busnummer met status inGebruik werd gehistoreerd door heradressering.")]
    public class AddressWasRetiredBecauseOfReaddress : IStreetNameEvent, IHasAddressPersistentLocalId
    {
        public const string EventName = "AddressWasRetiredBecauseOfReaddress"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van de straatnaam aan dewelke het adres is toegewezen.")]
        public int StreetNamePersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van het adres.")]
        public int AddressPersistentLocalId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public AddressWasRetiredBecauseOfReaddress(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            AddressPersistentLocalId = addressPersistentLocalId;
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
        }

        [JsonConstructor]
        private AddressWasRetiredBecauseOfReaddress(
            int streetNamePersistentLocalId,
            int addressPersistentLocalId,
            ProvenanceData provenance)
            : this(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                new AddressPersistentLocalId(addressPersistentLocalId))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(StreetNamePersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(AddressPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
