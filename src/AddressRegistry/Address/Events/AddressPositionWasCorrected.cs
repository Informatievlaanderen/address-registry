namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Newtonsoft.Json;
    using System;

    [EventName("AddressPositionWasCorrected")]
    [EventDescription("Het adres werd gepositioneerd via correctie.")]
    public class AddressPositionWasCorrected : IHasProvenance, ISetProvenance
    {
        public Guid AddressId { get; set; }
        public GeometryMethod GeometryMethod { get; set; }
        public GeometrySpecification GeometrySpecification { get; set; }
        public string ExtendedWkbGeometry { get; set; }
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
