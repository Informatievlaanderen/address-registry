namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    [Obsolete("This is a legacy event and should not be used anymore.")]
    [EventTags(EventTag.For.Sync)]
    [EventName("AddressWasRegistered")]
    [EventDescription("Het adres werd aangemaakt in het register.")]
    public class AddressWasRegistered : IHasProvenance, ISetProvenance, IMessage
    {
        [EventPropertyDescription("Interne GUID van het adres.")]
        public Guid AddressId { get; }

        [EventPropertyDescription("Interne GUID van de straatnaam die deel uitmaakt van het adres.")]
        public Guid StreetNameId { get; }

        [EventPropertyDescription("Huisnummer van het adres.")]
        public string HouseNumber { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public AddressWasRegistered(
            AddressId addressId,
            StreetNameId streetNameId,
            HouseNumber houseNumber)
        {
            AddressId = addressId;
            StreetNameId = streetNameId;
            HouseNumber = houseNumber;
        }

        [JsonConstructor]
        private AddressWasRegistered(
            Guid addressId,
            Guid streetNameId,
            string houseNumber,
            ProvenanceData provenance)
            : this(
                new AddressId(addressId),
                new StreetNameId(streetNameId),
                new HouseNumber(houseNumber))
                => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
