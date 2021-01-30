namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using System;

    [EventTags(EventTag.For.Sync)]
    [EventName("AddressBoxNumberWasCorrected")]
    [EventDescription("Het busnummer van het adres werd gecorrigeerd.")]
    public class AddressBoxNumberWasCorrected : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het adres.")]
        public Guid AddressId { get; }

        [EventPropertyDescription("Busnummer van het adres.")]
        public string BoxNumber { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public AddressBoxNumberWasCorrected(
            AddressId addressId,
            BoxNumber boxNumber)
        {
            AddressId = addressId;
            BoxNumber = boxNumber;
        }

        [JsonConstructor]
        private AddressBoxNumberWasCorrected(
            Guid addressId,
            string boxNumber,
            ProvenanceData provenance)
            : this(
                new AddressId(addressId),
                new BoxNumber(boxNumber))
                => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
