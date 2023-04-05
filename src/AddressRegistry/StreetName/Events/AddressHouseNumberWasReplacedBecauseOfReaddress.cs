namespace AddressRegistry.StreetName.Events
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using DataStructures;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Edit, EventTag.For.Sync)]
    [EventName(EventName)]
    [EventDescription("Het huisnummeradres werd gebruikt in een heradressering.")]
    public class AddressHouseNumberWasReplacedBecauseOfReaddress : IStreetNameEvent, IHasAddressPersistentLocalId
    {
        public const string EventName = "AddressHouseNumberWasReplacedBecauseOfReaddress"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van de straatnaam aan dewelke de adressen zijn toegewezen.")]
        public int StreetNamePersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van de doelstraatnaam aan dewelke de adressen zijn toegewezen.")]
        public int DestinationStreetNamePersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van het adres.")]
        public int AddressPersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van het doeladres.")]
        public int DestinationAddressPersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificatoren van het busnummeradressen.")]
        public IList<AddressBoxNumberReplacedBecauseOfReaddressData> BoxNumberAddressPersistentLocalIds { get;  }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public AddressHouseNumberWasReplacedBecauseOfReaddress(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            StreetNamePersistentLocalId destinationStreetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            AddressPersistentLocalId destinationAddressPersistentLocalId,
            IList<AddressBoxNumberReplacedBecauseOfReaddressData> boxNumberAddressPersistentLocalIds)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            DestinationStreetNamePersistentLocalId = destinationStreetNamePersistentLocalId;
            AddressPersistentLocalId = addressPersistentLocalId;
            DestinationAddressPersistentLocalId = destinationAddressPersistentLocalId;
            BoxNumberAddressPersistentLocalIds = boxNumberAddressPersistentLocalIds;
        }

        [JsonConstructor]
        private AddressHouseNumberWasReplacedBecauseOfReaddress(
            int streetNamePersistentLocalId,
            int destinationStreetNamePersistentLocalId,
            int addressPersistentLocalId,
            int destinationAddressPersistentLocalId,
            IList<AddressBoxNumberReplacedBecauseOfReaddressData> boxNumberAddressPersistentLocalIds,
            ProvenanceData provenance)
            : this(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                new StreetNamePersistentLocalId(destinationStreetNamePersistentLocalId),
                new AddressPersistentLocalId(addressPersistentLocalId),
                new AddressPersistentLocalId(destinationAddressPersistentLocalId),
                boxNumberAddressPersistentLocalIds)
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(StreetNamePersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(DestinationStreetNamePersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(AddressPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(DestinationAddressPersistentLocalId.ToString(CultureInfo.InvariantCulture));

            foreach (var item in BoxNumberAddressPersistentLocalIds)
            {
                fields.Add(item.SourceAddressPersistentLocalId.ToString());
                fields.Add(item.DestinationAddressPersistentLocalId.ToString());
            }

            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
