namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using System;

    [EventTags(EventTag.For.Sync)]
    [EventName("AddressStreetNameWasCorrected")]
    [EventDescription("Het adres werd aan een andere straatnaam toegekend (via correctie).")]
    public class AddressStreetNameWasCorrected : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het adres.")]
        public Guid AddressId { get; }

        [EventPropertyDescription("Interne GUID van de straatnaam die deel uitmaakt van het adres.")]
        public Guid StreetNameId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public AddressStreetNameWasCorrected(
            AddressId addressId,
            StreetNameId streetNameId)
        {
            StreetNameId = streetNameId;
            AddressId = addressId;
        }

        [JsonConstructor]
        private AddressStreetNameWasCorrected(
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
