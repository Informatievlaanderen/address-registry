namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using System;

    [Obsolete("This is a legacy event and should not be used anymore.")]
    [EventTags(EventTag.For.Sync)]
    [EventName("AddressHouseNumberWasCorrected")]
    [EventDescription("Het huisnummer van het adres werd gecorrigeerd.")]
    public class AddressHouseNumberWasCorrected : IHasProvenance, ISetProvenance, IMessage
    {
        [EventPropertyDescription("Interne GUID van het adres.")]
        public Guid AddressId { get; }

        [EventPropertyDescription("Huisnummer van het adres.")]
        public string HouseNumber { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public AddressHouseNumberWasCorrected(
            AddressId addressId,
            HouseNumber houseNumber)
        {
            AddressId = addressId;
            HouseNumber = houseNumber;
        }

        [JsonConstructor]
        private AddressHouseNumberWasCorrected(
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
