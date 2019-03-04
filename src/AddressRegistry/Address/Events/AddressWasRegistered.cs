namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    [EventName("AddressWasRegistered")]
    [EventDescription("Het adres werd geregistreerd.")]
    public class AddressWasRegistered : IHasProvenance, ISetProvenance
    {
        public Guid AddressId { get; }
        public Guid StreetNameId { get; }
        public string HouseNumber { get; }
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
