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
    [EventDescription("Het huisnummer van het adres werd gecorrigeerd.")]
    public class AddressHouseNumberWasCorrectedV2 : IStreetNameEvent, IHasAddressPersistentLocalId
    {
        public const string EventName = "AddressHouseNumberWasCorrectedV2"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van de straatnaam aan dewelke het adres is toegewezen.")]
        public int StreetNamePersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van het adres.")]
        public int AddressPersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificatoren van de gekoppelde busnummers.")]
        public IEnumerable<int> BoxNumberPersistentLocalIds { get; }

        [EventPropertyDescription("Huisnummer van het adres.")]
        public string HouseNumber { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public AddressHouseNumberWasCorrectedV2(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            IEnumerable<AddressPersistentLocalId> boxNumberPersistentLocalIds,
            HouseNumber houseNumber)
        {
            AddressPersistentLocalId = addressPersistentLocalId;
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            BoxNumberPersistentLocalIds = boxNumberPersistentLocalIds.Select(x => (int)x).ToList();
            HouseNumber = houseNumber;
        }

        [JsonConstructor]
        private AddressHouseNumberWasCorrectedV2(
            int streetNamePersistentLocalId,
            int addressPersistentLocalId,
            IEnumerable<int> boxNumberPersistentLocalIds,
            string houseNumber,
            ProvenanceData provenance)
            : this(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                new AddressPersistentLocalId(addressPersistentLocalId),
                boxNumberPersistentLocalIds.Select(x => new AddressPersistentLocalId(x)),
                new HouseNumber(houseNumber))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(StreetNamePersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(AddressPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(HouseNumber);
            fields.AddRange(BoxNumberPersistentLocalIds.Select(boxNumberPersistentLocalId => boxNumberPersistentLocalId.ToString()));

            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
