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
    [EventDescription("De postcode werd gecorrigeerd.")]
    public class AddressPostalCodeWasCorrectedV2 : IStreetNameEvent, IHasAddressPersistentLocalId
    {
        public const string EventName = "AddressPostalCodeWasCorrectedV2"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van de straatnaam aan dewelke het adres is toegewezen.")]
        public int StreetNamePersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van het adres.")]
        public int AddressPersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificatoren van de gekoppelde busnummers die de status voorgesteld of inGebruik hebben.")]
        public IEnumerable<int> BoxNumberPersistentLocalIds { get; }

        [EventPropertyDescription("Postcode (= objectidentificator) van het PostInfo-object dat deel uitmaakt van het adres.")]
        public string PostalCode { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public AddressPostalCodeWasCorrectedV2(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            IEnumerable<AddressPersistentLocalId> boxNumberPersistentLocalIds,
            PostalCode postalCode)
        {
            AddressPersistentLocalId = addressPersistentLocalId;
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            BoxNumberPersistentLocalIds = boxNumberPersistentLocalIds.Select(x => (int)x).ToList();
            PostalCode = postalCode;
        }

        [JsonConstructor]
        private AddressPostalCodeWasCorrectedV2(
            int streetNamePersistentLocalId,
            int addressPersistentLocalId,
            IEnumerable<int> boxNumberPersistentLocalIds,
            string postalCode,
            ProvenanceData provenance)
            : this(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                new AddressPersistentLocalId(addressPersistentLocalId),
                boxNumberPersistentLocalIds.Select(x => new AddressPersistentLocalId(x)),
                new PostalCode(postalCode))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(StreetNamePersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(AddressPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(PostalCode);
            fields.AddRange(BoxNumberPersistentLocalIds.Select(x => x.ToString()));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
