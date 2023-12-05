namespace AddressRegistry.Address.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Newtonsoft.Json;
    using System;

    [Obsolete("This is a legacy event and should not be used anymore.")]
    [EventTags(EventTag.For.Sync)]
    [EventName("AddressWasPositioned")]
    [EventDescription("De positie van het adres werd toegevoegd of gewijzigd.")]
    public class AddressWasPositioned : IHasProvenance, ISetProvenance, IMessage
    {
        [EventPropertyDescription("Interne GUID van het adres.")]
        public Guid AddressId { get; }

        [EventPropertyDescription("Geometriemethode van de adrespositie. Mogelijkheden: DerivedFromObject, AppointedByAdministrator of Interpolated.")]
        public GeometryMethod GeometryMethod { get; }

        [EventPropertyDescription("Specificatie van het object dat voorgesteld wordt door de adrespositie. Mogelijkheden: BuildingUnit, Stand, Parcel, Lot, Entry, RoadSegment, Municipality of Berth.")]
        public GeometrySpecification GeometrySpecification { get; }

        [EventPropertyDescription("Extended WKB-voorstelling van de adrespositie.")]
        public string ExtendedWkbGeometry { get; }

        [EventPropertyDescription("Metadata bij het event.")]
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
