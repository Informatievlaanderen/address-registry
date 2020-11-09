namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using System;

    [EventName("AddressStreetNameWasCorrected")]
    [EventDescription("De straat van het adres werd gewijzigd via correctie.")]
    public class AddressStreetNameWasCorrected : IHasProvenance, ISetProvenance
    {
        public Guid StreetNameId { get; }
        public Guid AddressId { get; }
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
