namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using System;

    [EventName("AddressPostalCodeWasCorrected")]
    [EventDescription("De postcode van het adres werd gewijzigd via correctie.")]
    public class AddressPostalCodeWasCorrected : IHasProvenance, ISetProvenance
    {
        public Guid AddressId { get; }
        public string PostalCode { get; }
        public ProvenanceData Provenance { get; private set; }

        public AddressPostalCodeWasCorrected(
            AddressId addressId,
            PostalCode postalCode)
        {
            AddressId = addressId;
            PostalCode = postalCode;
        }

        [JsonConstructor]
        private AddressPostalCodeWasCorrected(
            Guid addressId,
            string postalCode,
            ProvenanceData provenance)
            : this(
                new AddressId(addressId),
                new PostalCode(postalCode))
                => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
