namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using System;

    [EventTags(EventTag.For.Sync)]
    [EventName("AddressHouseNumberWasChanged")]
    [EventDescription("Het huisnummer van het adres werd gewijzigd.")]
    public class AddressHouseNumberWasChanged : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het adres.")]
        public Guid AddressId { get; }

        [EventPropertyDescription("Huisnummer van het adres.")]
        public string HouseNumber { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public AddressHouseNumberWasChanged(
            AddressId addressId,
            HouseNumber houseNumber)
        {
            AddressId = addressId;
            HouseNumber = houseNumber;
        }

        [JsonConstructor]
        private AddressHouseNumberWasChanged(
            Guid addressId,
            string houseNumber,
            ProvenanceData provenance)
            : this(
                new AddressId(addressId),
                new HouseNumber(houseNumber))
                => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
