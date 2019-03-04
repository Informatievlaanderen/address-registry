namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using System;

    [EventName("AddressStreetNameWasChanged")]
    [EventDescription("De straat van het adres werd gewijzigd.")]
    public class AddressStreetNameWasChanged : IHasProvenance, ISetProvenance
    {
        public Guid AddressId { get; }
        public Guid StreetNameId { get; }
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
