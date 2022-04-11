namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using System;

    [EventTags(EventTag.For.Sync)]
    [EventName("AddressStreetNameWasChanged")]
    [EventDescription("Het adres werd aan een andere straatnaam toegekend.")]
    public class AddressStreetNameWasChanged : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het adres.")]
        public Guid AddressId { get; }

        [EventPropertyDescription("Interne GUID van de straatnaam die deel uitmaakt van het adres.")]
        public Guid StreetNameId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public AddressStreetNameWasChanged(
            AddressId addressId,
            StreetNameId streetNameId)
        {
            AddressId = addressId;
            StreetNameId = streetNameId;
        }

        [JsonConstructor]
        private AddressStreetNameWasChanged(
            Guid addressId,
            Guid streetNameId,
            ProvenanceData provenance)
            : this(
                new AddressId(addressId),
                new StreetNameId(streetNameId))
                => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
