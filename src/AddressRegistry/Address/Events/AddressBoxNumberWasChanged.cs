namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using System;

    [EventName("AddressBoxNumberWasChanged")]
    [EventDescription("Het busnummer van het adres werd gewijzigd.")]
    public class AddressBoxNumberWasChanged : IHasProvenance, ISetProvenance
    {
        public Guid AddressId { get; }
        public string BoxNumber { get; }
        public ProvenanceData Provenance { get; private set; }

        public AddressBoxNumberWasChanged(
            AddressId addressId,
            BoxNumber boxNumber)
        {
            AddressId = addressId;
            BoxNumber = boxNumber;
        }

        [JsonConstructor]
        private AddressBoxNumberWasChanged(
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
