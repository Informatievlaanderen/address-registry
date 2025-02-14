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
    [EventTags(EventTag.For.Edit)]
    [EventName(EventName)]
    [EventDescription("De busnummers werden gecorrigeerd.")]
    public class AddressBoxNumbersWereCorrected : IStreetNameEvent
    {
        public const string EventName = "AddressBoxNumbersWereCorrected"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van de straatnaam aan dewelke de adressen zijn toegewezen.")]
        public int StreetNamePersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van het adres met het nieuwe busnummer.")]
        public IDictionary<int, string> AddressBoxNumbers { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public AddressBoxNumbersWereCorrected(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            IDictionary<AddressPersistentLocalId, BoxNumber> addressBoxNumbers)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            AddressBoxNumbers = addressBoxNumbers.ToDictionary(x => (int)x.Key, x => x.Value.ToString());
        }

        [JsonConstructor]
        private AddressBoxNumbersWereCorrected(
            int streetNamePersistentLocalId,
            IDictionary<int, string> addressBoxNumbers,
            ProvenanceData provenance)
            : this(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                addressBoxNumbers.ToDictionary(x => new AddressPersistentLocalId(x.Key), x => new BoxNumber(x.Value)))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(StreetNamePersistentLocalId.ToString(CultureInfo.InvariantCulture));

            foreach (var addressBoxNumber in AddressBoxNumbers)
            {
                fields.Add(addressBoxNumber.Key.ToString(CultureInfo.InvariantCulture));
                fields.Add(addressBoxNumber.Value);
            }

            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
