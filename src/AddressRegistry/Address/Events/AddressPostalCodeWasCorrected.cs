namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using System;

    [Obsolete("This is a legacy event and should not be used anymore.")]
    [EventTags(EventTag.For.Sync)]
    [EventName("AddressPostalCodeWasCorrected")]
    [EventDescription("De postcode van het adres werd gecorrigeerd.")]
    public class AddressPostalCodeWasCorrected : IHasProvenance, ISetProvenance, IMessage
    {
        [EventPropertyDescription("Interne GUID van het adres.")]
        public Guid AddressId { get; }

        [EventPropertyDescription("Postcode (= objectidentificator) van het PostInfo-object dat deel uitmaakt van het adres.")]
        public string PostalCode { get; }

        [EventPropertyDescription("Metadata bij het event.")]
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
