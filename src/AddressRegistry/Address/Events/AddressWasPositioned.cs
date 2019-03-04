namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Newtonsoft.Json;
    using System;

    [EventName("AddressWasPositioned")]
    [EventDescription("Het adres werd gepositioneerd.")]
    public class AddressWasPositioned : IHasProvenance, ISetProvenance
    {
        public Guid AddressId { get; }
        public GeometryMethod GeometryMethod { get; }
        public GeometrySpecification GeometrySpecification { get; }
        public string ExtendedWkbGeometry { get; }
        public ProvenanceData Provenance { get; private set; }

        public AddressWasPositioned(
            AddressId addressId,
            AddressGeometry addressGeometry)
        {
            AddressId = addressId;
            GeometryMethod = addressGeometry.GeometryMethod;
            GeometrySpecification = addressGeometry.GeometrySpecification;
            ExtendedWkbGeometry = addressGeometry.Geometry.ToString();
        }

        [JsonConstructor]
        private AddressWasPositioned(
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
