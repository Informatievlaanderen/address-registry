namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Newtonsoft.Json;
    using System;

    [EventTags(EventTag.For.Sync)]
    [EventName("AddressPositionWasCorrected")]
    [EventDescription("De adrespositie werd gecorrigeerd.")]
    public class AddressPositionWasCorrected : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het adres.")]
        public Guid AddressId { get; set; }

        [EventPropertyDescription("Geometriemethode van de adrespositie.")]
        public GeometryMethod GeometryMethod { get; set; }

        [EventPropertyDescription("Specificatie van het object dat voorgesteld wordt door de adrespositie.")]
        public GeometrySpecification GeometrySpecification { get; set; }

        [EventPropertyDescription("Extended WKB-voorstelling van de adrespositie.")]
        public string ExtendedWkbGeometry { get; set; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public AddressPositionWasCorrected(
            AddressId addressId,
            AddressGeometry addressGeometry)
        {
            AddressId = addressId;
            GeometryMethod = addressGeometry.GeometryMethod;
            GeometrySpecification = addressGeometry.GeometrySpecification;
            ExtendedWkbGeometry = addressGeometry.Geometry.ToString();
        }

        [JsonConstructor]
        private AddressPositionWasCorrected(
            Guid addressId,
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification,
            string extendedWkbGeometry,
            ProvenanceData provenance)
            : this(
                new AddressId(addressId),
                new AddressGeometry(geometryMethod, geometrySpecification, new ExtendedWkbGeometry(extendedWkbGeometry.ToByteArray())))
                => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
